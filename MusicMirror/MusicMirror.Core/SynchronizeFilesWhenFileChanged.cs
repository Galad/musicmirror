using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hanno.Services;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;

namespace MusicMirror
{
	public sealed class SynchronizeFilesWhenFileChanged : IStartSynchronizing, ITranscodingNotifications
	{
		private readonly IObservable<MusicMirrorConfiguration> _configurationObservable;
		private readonly IFileObserverFactory _fileObserverFactory;
		private readonly IFileSynchronizerVisitorFactory _fileSynchronizerVisitorFactory;
        private readonly Subject<IFileTranscodingResultNotification> _transcodingResultNotifications;
        private readonly Subject<IFileNotification[]> _fileNotifications;
        private readonly IObservable<bool> _isTranscodingRunning;
        private readonly CompositeDisposable _subscribtions;
        private readonly IScheduler _scheduler;

        public IObservable<MusicMirrorConfiguration> ConfigurationObservable
        {
            get
            {
                return _configurationObservable;
            }
        }

        public IFileObserverFactory FileObserverFactory
        {
            get
            {
                return _fileObserverFactory;
            }
        }

        public IFileSynchronizerVisitorFactory FileSynchronizerVisitorFactory
        {
            get
            {
                return _fileSynchronizerVisitorFactory;
            }
        }

        public SynchronizeFilesWhenFileChanged(
			IObservable<MusicMirrorConfiguration> configurationObservable,
			IFileObserverFactory fileObserverFactory,
			IFileSynchronizerVisitorFactory fileSynchronizerVisitorFactory,
            IScheduler scheduler)
		{
			if (configurationObservable == null) throw new ArgumentNullException(nameof(configurationObservable));
			if (fileObserverFactory == null) throw new ArgumentNullException(nameof(fileObserverFactory));
			if (fileSynchronizerVisitorFactory == null) throw new ArgumentNullException(nameof(fileSynchronizerVisitorFactory));
            _subscribtions = new CompositeDisposable();
			_configurationObservable = configurationObservable;
			_fileObserverFactory = fileObserverFactory;
			_fileSynchronizerVisitorFactory = fileSynchronizerVisitorFactory;
            _transcodingResultNotifications = new Subject<IFileTranscodingResultNotification>();
            _fileNotifications = new Subject<IFileNotification[]>();
            _isTranscodingRunning = ObserveIsTranscodinningRunningCold().ReplayAndConnect(1, _subscribtions, ImmediateScheduler.Instance);
            _scheduler = scheduler;                
		}

		public IDisposable Subscribe()
		{
			return ConfigurationObservable				
				.Select(ObserveFiles)
				.Switch()
				.Subscribe();
		}

		private IObservable<Unit> ObserveFiles(MusicMirrorConfiguration configuration)
		{
			var visitor = FileSynchronizerVisitorFactory.CreateVisitor(configuration);						
			return FileObserverFactory.GetFileObserver(configuration.SourcePath)
                                      .Do(files => _fileNotifications.OnNext(files))                                      
									  .SelectMany(files => files.Select(file => SynchronizeFile(file, visitor))
										    					 .Merge(4)
																 .ToList()
																 .SelectUnit());
		}

		private IObservable<Unit> SynchronizeFile(IFileNotification file, IFileSynchronizerVisitor visitor)
		{
            return Observable.FromAsync(async ct => await file.Accept(ct, visitor).ContinueWith(_ => { }, TaskContinuationOptions.ExecuteSynchronously))                             
                             .Do(_ => _transcodingResultNotifications.OnNext(FileTranscodingResultNotification.CreateSuccess(file)))
                             .Catch((Exception ex) =>
                             {
                                 _transcodingResultNotifications.OnNext(FileTranscodingResultNotification.CreateFailure(file, ex));
                                 return Observable.Return(Unit.Default, ImmediateScheduler.Instance);
                             });
		}

        public IObservable<IFileNotification[]> ObserveNotifications()
        {
            return _fileNotifications.AsObservable();
        }

        public IObservable<IFileTranscodingResultNotification> ObserveTranscodingResult()
        {
            return _transcodingResultNotifications.AsObservable();
        }

        public IDisposable Start()
        {
            return Subscribe();
        }

        public IObservable<bool> ObserveIsTranscodingRunning()
        {
            return _isTranscodingRunning;
        }

        private IObservable<bool> ObserveIsTranscodinningRunningCold()
        {
            return Observable.Merge(
                            ObserveNotifications().Select(f => f.Length),
                            ObserveTranscodingResult().Select(f => -1)
                            )
                            .StartWith(ImmediateScheduler.Instance, 0)
                            .Scan(0, (x, y) => x + y)
                            .Select(filesInQueue => filesInQueue > 0);
                            //.DistinctUntilChanged();
        }
    }
}