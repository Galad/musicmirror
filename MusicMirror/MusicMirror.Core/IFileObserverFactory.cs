using System;
using System.IO;

namespace MusicMirror
{
	public interface IFileObserverFactory
	{
		IObservable<IFileNotification[]> GetFileObserver(DirectoryInfo path);
	}
}