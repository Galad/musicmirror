using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Hanno;
using Hanno.Concurrency;
using Hanno.CqrsInfrastructure;
using Hanno.Diagnostics;
using Hanno.IO;
using Hanno.MVVM.Services;
using Hanno.Navigation;
using Hanno.Rx;
using Hanno.Serialization;
using Hanno.Services;
using Hanno.Storage;
using Hanno.Unity;
using Hanno.Validation;
using Hanno.ViewModels;
using Microsoft.Practices.Unity;
using MusicMirror.Logging;
using MusicMirror.Synchronization;
using MusicMirror.Transcoding;
using MusicMirror.ViewModels;
using System.Reactive;
using NAudio.MediaFoundation;
using System.Threading;
using NLog;

namespace MusicMirror
{
    public sealed class AppComposer : Composer
    {
        public AppComposer() : base(
            new CompositeCompositionModule(
                new LoggingComposer(),
                new SchedulersModule()))
        {
        }
    }

    public abstract class Composer : IDisposable
    {
        private readonly UnityContainer _container;        
        private readonly ICompositionModule _extraModule;

        protected Composer(ICompositionModule extraModule)
        {
            _extraModule = Guard.ForNull(extraModule, nameof(extraModule));
            _container = new UnityContainer();            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Composition root")]
        public ConfigurationPageViewModel Compose()
        {
            MediaFoundationApiStarter.Start();
            _extraModule.Compose(_container);            
            var cqrs = new AsyncCommandQueryBus(
                new UnityCommandQueryHandlerFactory(_container),
                new UnityCommandQueryHandlerFactory(_container));
            var serializer = new XmlSerializer();
            _container.RegisterType(
                typeof(IAsyncDataTable<,>),
                typeof(AsyncFromStringKeyValuePairFilesDataTable<,>),
                "Settings",
                new InjectionConstructor(
                    new InjectionParameter(new SafeStringSerializer(serializer)),
                    new InjectionParameter(new SafeStringDeserializer(serializer)),
                    new InjectionParameter(new AsyncOperations(new FileOperations())),
                    new InjectionParameter(new AsyncDirectoryOperations(new DirectoryOperations())),
                    new InjectionParameter(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Settings")),
                    new InjectionParameter(typeof(string), default(string))));
            _container.RegisterType<ISettingsService>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new SettingsService(
                c.Resolve<ISchedulers>().ThreadPool,
                new AsyncStorage(
                    new UnityAsyncDataTableFactory(
                        c,
                        "Settings")))));

            _container.RegisterType<SynchronizeFilesWhenFileChanged>(
            new ContainerControlledLifetimeManager(),
            new InjectionFactory(c =>
                new SynchronizeFilesWhenFileChanged(
                    GetConfigurationObservable(c.Resolve<ISettingsService>()),
                    new LoggingFileObserverFactory(
                        new FileObserverFactory(new FileWatcher()),
                        c.Resolve<Func<string, ILogger>>()("FileObserver")),
                    new LoggingFileSynchronizerVisitorFactory(
                        new FileSynchronizerVisitorFactory(CreateTranscoder(c)),
                        //new EmptyFileSynchronizerVisitorFactory(),
                        c.Resolve<Func<string, ILogger>>()("IFileSynchronizerVisitor")
                        ), 
                    c.Resolve<ISchedulers>().ThreadPool,
                    c.Resolve<IScheduler>(Constants.Schedulers.NotificationsScheduler))));

            _container.RegisterType<ITranscodingNotifications, SynchronizeFilesWhenFileChanged>();
            _container.RegisterType<IStartSynchronizing, SynchronizeFilesWhenFileChanged>();

            _container.RegisterType<ISynchronizationController>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new SynchronizationController(
                    c.Resolve<ISchedulers>().ThreadPool,
                    _container.Resolve<IStartSynchronizing>(),
                    _container.Resolve<ITranscodingNotifications>())
                ));

            _container.RegisterType<ConfigurationPageViewModel>(
                new InjectionFactory(c =>
                {
                    var viewModel = new ConfigurationPageViewModel(
                        new ViewModelServices(
                            new NoRuleProvider(),
                            new DefaultObservableRegistrationService(
                                new MessageBoxAsyncMessageDialog(),
                                new StringResources()),
                            new PropertyValidator(),
                            c.Resolve<ISchedulers>(),
                            new EmptyNavigationService(),
                            new EmptyRequestNavigation(),
                            cqrs,
                            cqrs,
                            new CommandEvents(),
                            new NotifyCommandStateBus(cqrs, c.Resolve<ISchedulers>().ThreadPool),
                            new NotifyQueryStateBus(cqrs)),
                            c.Resolve<ISettingsService>(),
                            _container.Resolve<ISynchronizationController>(),
                            _container.Resolve<ITranscodingNotifications>(),
                            c.Resolve<Func<string, ILogger>>()("ConfigurationPageViewModel")
                        );
                    viewModel.Initialize(new NavigationRequest("Main", new Dictionary<string, string>()));
                    return viewModel;
                }));

            ObserveAndLog();
            return _container.Resolve<ConfigurationPageViewModel>();
        }

        private void ObserveAndLog()
        {
            var logger = _container.Resolve<Func<string, ILogger>>()("ITranscodingNotifications");
            var notifications = _container.Resolve<ITranscodingNotifications>();
            notifications.ObserveIsTranscodingRunning()
                         .Subscribe(isRunning =>
                         {
                             if (isRunning)
                             {
                                 logger.Info("Transcoding is running");
                             }
                             else
                             {
                                 logger.Info("Transcoding is stopped");
                             }
                         });
            notifications.ObserveTranscodingResult()
                         .Subscribe(n =>
                         {
                             n.HandleResult(
                                 () => logger.Info("Transcoding result notification : Success"),
                                 ex => logger.Error(ex, "Transcoding result notification : Error")
                                 );
                         });
        }

        private static IObservable<MusicMirrorConfiguration> GetConfigurationObservable(ISettingsService settingsService)
        {
            return new FilterValidDirectories(new ConfigurationObservable(settingsService));
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public T Resolve<T>(string name)
        {
            return _container.Resolve<T>(name);
        }

        private static IFileTranscoder CreateTranscoder(IUnityContainer container)
        {
            var transcoder = new TranscoderDispatch(new DebugFileTranscoder());
            transcoder.AddTranscoder(
                new CopyId3TagsPostProcessor(
                new NAudioFileTranscoder(
                    new FlacStreamReader(),
                    //new FlacStreamReaderInternalNAudioFlac(),
                    new WaveToMP3Transcoder(),
                    //new RawWaveTranscoder(),
                    //new WaveToMP3MediaFoundationTranscoder(),
                    new AsyncOperations(new FileOperations()),
                    new AsyncDirectoryOperations(new DirectoryOperations())
                    ),
                new AudioTagsSynchronizer(
                    new AsyncOperations(new FileOperations()),
                    new FlacTagLibReaderWriter(),
                    new MP3TagLibReaderWriter()
                )),
                AudioFormat.Flac);
            return new LoggingFileTranscoder(transcoder, container.Resolve<Func<string, ILogger>>()("IFileTranscoder"));
        }

        #region IDisposable
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _container.Dispose();
                MediaFoundationApi.Shutdown();
                // Free any other managed objects here. 
                //
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
        }

        #endregion
    }

    public static class MediaFoundationApiStarter
    {
        private static int _isStarted;

        public static void Start()
        {
            var isStarted = Interlocked.Exchange(ref _isStarted, 1);
            if (isStarted == 0)
            {
                MediaFoundationApi.Startup();
            }
        }
    }
}
