using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
	public sealed class SynchronizationController : ISynchronizationController, ISynchronizationNotifications
	{
		private readonly IScheduler _scheduler;
		private readonly ISubject<bool> _isEnabled;

		public IScheduler Scheduler
		{
			get
			{
				return _scheduler;
			}
		}

		public SynchronizationController(IScheduler scheduler)
		{
			if (scheduler == null)throw new ArgumentNullException(nameof(scheduler));
			_scheduler = scheduler;
			_isEnabled = new ReplaySubject<bool>(1, _scheduler);
			_isEnabled.OnNext(false);
		}

		public IObservable<IObservable<FileSynchronizationResult>> ObserveSynchronizationNotifications()
		{
			return Observable.Return(Observable.Empty<FileSynchronizationResult>());
			//return Observable.Empty<IObservable<FileSynchronizationResult>>();
            //return Observable.Never<IObservable<FileSynchronizationResult>>();
        }

		public void Enable()
		{
			_isEnabled.OnNext(true);			
		}

		public void Disable()
		{
			_isEnabled.OnNext(false);
		}

		public IObservable<bool> ObserveSynchronizationIsEnabled()
		{
			return _isEnabled.AsObservable();
		}		
	}
}
