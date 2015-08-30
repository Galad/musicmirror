using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hanno.Services;
using MusicMirror.Entities;

namespace MusicMirror
{
	public sealed class SynchronizeFilesWhenFileChanged : IObservable<Unit>
	{
		private readonly IObservable<MusicMirrorConfiguration> _configurationObservable;
		private readonly IFileObserverFactory _fileObserverFactory;
		private readonly IFileSynchronizerVisitorFactory _fileSynchronizerVisitorFactory;

		public SynchronizeFilesWhenFileChanged(
			IObservable<MusicMirrorConfiguration> configurationObservable,
			IFileObserverFactory fileObserverFactory,
			IFileSynchronizerVisitorFactory fileSynchronizerVisitorFactory)
		{
			if (configurationObservable == null) throw new ArgumentNullException(nameof(configurationObservable));
			if (fileObserverFactory == null) throw new ArgumentNullException(nameof(fileObserverFactory));
			if (fileSynchronizerVisitorFactory == null) throw new ArgumentNullException(nameof(fileSynchronizerVisitorFactory));
			_configurationObservable = configurationObservable;
			_fileObserverFactory = fileObserverFactory;
			_fileSynchronizerVisitorFactory = fileSynchronizerVisitorFactory;
		}

		public IDisposable Subscribe(IObserver<Unit> observer)
		{
			return _configurationObservable				
				.Select(ObserveFiles)
				.Switch()
				.Subscribe(observer);
		}

		private IObservable<Unit> ObserveFiles(MusicMirrorConfiguration configuration)
		{
			var visitor = _fileSynchronizerVisitorFactory.CreateVisitor(configuration);						
			return _fileObserverFactory.GetFileObserver(configuration.SourcePath)
									   .SelectMany(files => files.Select(file => SynchronizeFile(file, visitor))
																 .Merge(4)
																 .ToList()
																 .SelectUnit());
		}

		private static IObservable<Unit> SynchronizeFile(IFileNotification file, IFileSynchronizerVisitor visitor)
		{
			return Observable.FromAsync(async ct => await file.Accept(ct, visitor))
							 .Catch(Observable.Return(Unit.Default));
		}
	}
}