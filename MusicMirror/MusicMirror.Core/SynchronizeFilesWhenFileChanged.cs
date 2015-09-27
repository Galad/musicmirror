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
using System.Diagnostics;

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
        private IScheduler _synchronizationScheduler;
        private readonly ReplaySubject<int> _numberOfFilesAddedInTranscodingQueue;
        private readonly IScheduler _notificationsScheduler;

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

        public IScheduler SynchronizationScheduler
        {
            get
            {
                return _synchronizationScheduler;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(SynchronizationScheduler));
                _synchronizationScheduler = value;
            }
        }

        public IScheduler NotificationsScheduler
        {
            get
            {
                return _notificationsScheduler;
            }
        }

        public SynchronizeFilesWhenFileChanged(
            IObservable<MusicMirrorConfiguration> configurationObservable,
            IFileObserverFactory fileObserverFactory,
            IFileSynchronizerVisitorFactory fileSynchronizerVisitorFactory,
            IScheduler synchronizationScheduler,
            IScheduler notificationsScheduler)
        {
            if (configurationObservable == null) throw new ArgumentNullException(nameof(configurationObservable));
            if (fileObserverFactory == null) throw new ArgumentNullException(nameof(fileObserverFactory));
            if (fileSynchronizerVisitorFactory == null) throw new ArgumentNullException(nameof(fileSynchronizerVisitorFactory));
            if (synchronizationScheduler == null) throw new ArgumentNullException(nameof(synchronizationScheduler));
            if (notificationsScheduler == null) throw new ArgumentNullException(nameof(notificationsScheduler));            
            _subscribtions = new CompositeDisposable();
            _configurationObservable = configurationObservable;
            _fileObserverFactory = fileObserverFactory;
            _fileSynchronizerVisitorFactory = fileSynchronizerVisitorFactory;
            _transcodingResultNotifications = new Subject<IFileTranscodingResultNotification>();
            _fileNotifications = new Subject<IFileNotification[]>();
            _numberOfFilesAddedInTranscodingQueue = new ReplaySubject<int>(1, ImmediateScheduler.Instance).DisposeWith(_subscribtions);
            _numberOfFilesAddedInTranscodingQueue.OnNext(0);
            _isTranscodingRunning = ObserveIsTranscodinningRunningCold().ReplayAndConnect(1, _subscribtions, ImmediateScheduler.Instance);            
            _synchronizationScheduler = synchronizationScheduler;
            _notificationsScheduler = notificationsScheduler;
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
                                      .ObserveOn(_notificationsScheduler)                                                       
                                      .Do(files =>
                                      {                                          
                                          _numberOfFilesAddedInTranscodingQueue.OnNext(files.Length);                                          
                                          _fileNotifications.OnNext(files);                                          
                                      })
                                      .SelectMany(files => files.Select(file => SynchronizeFile(file, visitor))
                                                                 .Merge(4, _synchronizationScheduler)
                                                                 .ToList()
                                                                 .SelectUnit());
        }

        private IObservable<Unit> SynchronizeFile(IFileNotification file, IFileSynchronizerVisitor visitor)
        {
            return Observable.FromAsync(async ct => await file.Accept(ct, visitor), _notificationsScheduler)                             
                             .Do(_ =>
                             {
                                 _transcodingResultNotifications.OnNext(FileTranscodingResultNotification.CreateSuccess(file));
                                 _numberOfFilesAddedInTranscodingQueue.OnNext(-1);
                             })
                             .Catch((Exception ex) =>
                             {
                                 _transcodingResultNotifications.OnNext(FileTranscodingResultNotification.CreateFailure(file, ex));
                                 _numberOfFilesAddedInTranscodingQueue.OnNext(-1);
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
            return _numberOfFilesAddedInTranscodingQueue
                            .Scan(0, (x, y) => x + y)
                            .Select(filesInQueue => filesInQueue > 0)
                            .DistinctUntilChanged();
        }
    }
}