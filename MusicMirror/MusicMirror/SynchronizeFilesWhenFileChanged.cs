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
		private readonly IObservable<Configuration> _configurationObservable;
		private readonly IFileObserverFactory _fileObserverFactory;
		private readonly IFileSynchronizerVisitorFactory _fileSynchronizerVisitorFactory;

		public SynchronizeFilesWhenFileChanged(
			IObservable<Configuration> configurationObservable,
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
				.Where(c => c.HasValidDirectories)
				.DistinctUntilChanged()
				.Select(ObserveFiles)
				.Switch()
				.Subscribe(observer);
		}

		private IObservable<Unit> ObserveFiles(Configuration configuration)
		{
			var visitor = _fileSynchronizerVisitorFactory.CreateVisitor(configuration);
			return _fileObserverFactory.GetFileObserver(configuration.SourcePath)
									   .SelectMany(files => files.Select(file => SynchronizeFile(file, visitor))
																 .Merge(4));
		}

		private static IObservable<Unit> SynchronizeFile(IFileNotification file, IFileSynchronizerVisitor visitor)
		{
			return Observable.FromAsync(async ct => await file.Accept(ct, visitor))
							 .Catch(Observable.Return(Unit.Default));
		}
	}
}