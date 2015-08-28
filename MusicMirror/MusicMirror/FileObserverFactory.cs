using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MusicMirror
{
	public sealed class FileObserverFactory : IFileObserverFactory
	{
		private readonly IFileWatcher _fileWatcher;

		public FileObserverFactory(IFileWatcher fileWatcher)
		{
			if (fileWatcher == null) throw new ArgumentNullException(nameof(fileWatcher));
			_fileWatcher = fileWatcher;
		}

		public IObservable<IFileNotification[]> GetFileObserver(DirectoryInfo path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));
			var getAllFiles = Observable.FromAsync(async ct =>
			{
				var files = await Task.Run(() => Directory.GetFiles(path.FullName, "*.*", SearchOption.AllDirectories), ct);
				return files.Select(f => (IFileNotification)new FileInitNotification(new FileInfo(f))).ToArray();
			});
			var watchFiles = _fileWatcher.WatchFiles(path);
			return getAllFiles.Merge(watchFiles.Select(fn => new[] {fn}));
		}
	}
}