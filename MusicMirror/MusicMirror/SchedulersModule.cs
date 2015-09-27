using System;
using Microsoft.Practices.Unity;
using Hanno;
using System.Reactive.Concurrency;
using Hanno.Concurrency;
using Hanno.Rx;

namespace MusicMirror
{
    public class SchedulersModule : ICompositionModule
    {
        public SchedulersModule()
        {
        }

        public void Compose(IUnityContainer container)
        {
            container.RegisterType<ISchedulers>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new WpfSchedulers(
                        DispatcherScheduler.Current,
                        new SingleSchedulerPriorityScheduler(DispatcherScheduler.Current),
                            new SingleSchedulerPriorityScheduler(ThreadPoolScheduler.Instance))));
            container.RegisterType<IScheduler>(
                Constants.Schedulers.NotificationsScheduler,
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new EventLoopScheduler(t => new System.Threading.Thread(t) { Name = Constants.Schedulers.NotificationsScheduler })));            
        }
    }
}