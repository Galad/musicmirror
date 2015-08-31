using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
	public sealed class SynchronizationController : ISynchronizationController, ISynchronizationNotifications
	{
		public IObservable<IObservable<FileSynchronizationResult>> ObserveSynchronizationNotifications()
		{
			return Observable.Return(Observable.Empty<FileSynchronizationResult>());
			//return Observable.Empty<IObservable<FileSynchronizationResult>>();
            //return Observable.Never<IObservable<FileSynchronizationResult>>();
        }

		public IDisposable Enable()
		{
			return Disposable.Empty;
		}

		public IObservable<bool> ObserveSynchronizationIsEnabled()
		{
			return Observable.Never<bool>();
		}
	}
}
