using System;
using System.IO;
using System.Reactive.Linq;

namespace MusicMirror
{
	public sealed class FileWatcher : IFileWatcher
	{
		public IObservable<IFileNotification> WatchFiles(DirectoryInfo directory)
		{
			return Observable.Using(() => new FileSystemWatcher(directory.FullName), fileWatcher =>
			{
				fileWatcher.EnableRaisingEvents = true;
				var fileAdded = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
					h => fileWatcher.Created += h,
					h => fileWatcher.Created -= h)
				                          .Select(args => new FileAddedNotification(new FileInfo(args.EventArgs.FullPath)));
				var fileModified = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
					h => fileWatcher.Changed += h,
					h => fileWatcher.Changed -= h)
				                             .Select(args => new FileModifiedNotification(new FileInfo(args.EventArgs.FullPath)));
				var fileDeleted = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
					h => fileWatcher.Deleted += h,
					h => fileWatcher.Deleted -= h)
				                            .Select(args => new FileDeletedNotification(new FileInfo(args.EventArgs.FullPath)));
				var fileRenamed = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
					h => fileWatcher.Renamed += h,
					h => fileWatcher.Renamed -= h)
				                            .Select(args => new FileRenamedNotification(new FileInfo(args.EventArgs.FullPath), args.EventArgs.OldFullPath));
				var error = Observable.FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
					h => fileWatcher.Error += h,
					h => fileWatcher.Error -= h)
				                      .SelectMany(args => Observable.Throw<IFileNotification>(args.EventArgs.GetException()));
				return Observable.Merge<IFileNotification>(fileAdded, fileModified, fileDeleted, fileRenamed, error);
			});
		}
	}
}