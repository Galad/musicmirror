using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
	public interface ISynchronizationController
	{
		IDisposable Enable();
		IObservable<bool> ObserveSynchronizationIsEnabled();
	}

	public interface ISynchronizationNotifications
	{		
		IObservable<IObservable<FileSynchronizationResult>> ObserveSynchronizationNotifications();
	}
}
