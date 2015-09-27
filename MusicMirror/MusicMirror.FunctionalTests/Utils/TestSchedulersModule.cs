using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Hanno;
using Hanno.Concurrency;
using System.Reactive.Concurrency;

namespace MusicMirror.FunctionalTests.Utils
{
    public sealed class TestSchedulersModule : ICompositionModule
    {
        public void Compose(IUnityContainer container)
        {
            container.RegisterType<ISchedulers>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new SingleSchedulers(new SingleSchedulerPriorityScheduler(ThreadPoolScheduler.Instance)))
                );
            container.RegisterType<IScheduler>(
                Constants.Schedulers.NotificationsScheduler,
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new EventLoopScheduler(t => new System.Threading.Thread(t) { Name = Constants.Schedulers.NotificationsScheduler })));
        }

        private sealed class SingleSchedulers : ISchedulers
        {
            private readonly IPriorityScheduler _scheduler;

            public SingleSchedulers(IPriorityScheduler scheduler)
            {
                _scheduler = scheduler;
            }

            public IScheduler CurrentThread => _scheduler;
            public IPriorityScheduler Dispatcher => new SingleSchedulerPriorityScheduler(ImmediateScheduler.Instance);
            public IScheduler Immediate => ImmediateScheduler.Instance;
            public IScheduler TaskPool => _scheduler;
            public IPriorityScheduler ThreadPool => _scheduler;

            public void SafeDispatch(Action action)
            {
                action();
            }
        }
    }  
}
