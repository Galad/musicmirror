using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
    public sealed class SynchronizationController : ISynchronizationController, IDisposable
    {
        private readonly IScheduler _scheduler;
        private readonly ReplaySubject<IDisposable> _enabledDisposable;
        private readonly IStartSynchronizing _startSynchronizing;
        private readonly IDisposable _disposable;
        private readonly ITranscodingNotifications _transcodingNotifications;

        public IScheduler Scheduler
        {
            get
            {
                return _scheduler;
            }
        }

        public IStartSynchronizing StartSynchronizing
        {
            get
            {
                return _startSynchronizing;
            }
        }

        public ITranscodingNotifications TranscodingNotifications
        {
            get
            {
                return _transcodingNotifications;
            }
        }

        public SynchronizationController(
            IScheduler scheduler, 
            IStartSynchronizing startSynchronizing,
            ITranscodingNotifications transcodingNotifications)
        {
            if (transcodingNotifications == null) throw new ArgumentNullException(nameof(transcodingNotifications), $"{nameof(transcodingNotifications)} is null.");
            if (startSynchronizing == null) throw new ArgumentNullException(nameof(startSynchronizing), $"{nameof(startSynchronizing)} is null.");
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            _scheduler = scheduler;
            _startSynchronizing = startSynchronizing;
            _transcodingNotifications = transcodingNotifications;
            _enabledDisposable = new ReplaySubject<IDisposable>(1, _scheduler);
            _enabledDisposable.OnNext(null);
            _disposable = _enabledDisposable.Delta((d1, d2) =>
            {
                if (d1 != null)
                {                    
                    d1.Dispose();
                }
                return d1 != null || d2 != null;
            })
                .TakeWhile(b => b)
                .SubscribeOn(_scheduler)
                .Subscribe(_ => { }, e => { });
        }

        public void Enable()
        {
            _enabledDisposable.OnNext(StartSynchronizing.Start());
        }

        public void Disable()
        {
            _enabledDisposable.OnNext(null);
        }

        public IObservable<bool> ObserveSynchronizationIsEnabled()
        {
            return _enabledDisposable.Select(d => d != null);
        }

        #region IDisposable Support
        private bool disposedValue = false;        

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _enabledDisposable.OnNext(null);
                    _enabledDisposable.OnNext(null);
                    _enabledDisposable.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }        
        #endregion
    }
}
