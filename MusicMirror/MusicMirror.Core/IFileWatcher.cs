using System;
using System.IO;

namespace MusicMirror
{
	public interface IFileWatcher
	{
		IObservable<IFileNotification> WatchFiles(DirectoryInfo directory);
	}
}