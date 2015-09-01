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
using log4net;
using log4net.Appender;
using Microsoft.Practices.Unity;
using MusicMirror.Logging;
using MusicMirror.Synchronization;
using MusicMirror.Transcoding;
using MusicMirror.ViewModels;

using System.Reactive;
using NAudio.MediaFoundation;

namespace MusicMirror
{
	public sealed class AppComposer : Composer
	{
		public AppComposer() : base(() => new WpfSchedulers(
			DispatcherScheduler.Current,
			new SingleSchedulerPriorityScheduler(DispatcherScheduler.Current),
			new SingleSchedulerPriorityScheduler(ThreadPoolScheduler.Instance)))
		{
		}
	}
	public abstract class Composer : IDisposable
	{
		private readonly UnityContainer _container;
		private readonly Func<ISchedulers> _schedulers;

		protected Composer(Func<ISchedulers> schedulers)
		{
			_schedulers = Guard.ForNull(schedulers, nameof(schedulers));
			_container = new UnityContainer();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Composition root")]
		public ConfigurationPageViewModel Compose()
		{
			MediaFoundationApi.Startup();
			var schedulers = _schedulers();
			log4net.Config.XmlConfigurator.Configure();
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
			var settingsService = new SettingsService(
				schedulers.ThreadPool,
				new AsyncStorage(
					new UnityAsyncDataTableFactory(
						_container,
						"Settings")));

			_container.RegisterType<IObservable<Unit>>(
			"SynchronizeFilesWhenFileChanged",
			new ContainerControlledLifetimeManager(),
			new InjectionFactory(c =>
		new SynchronizeFilesWhenFileChanged(
			GetConfigurationObservable(settingsService),
			new FileObserverFactory(new FileWatcher()),
			new LoggingFileSynchronizerVisitorFactory(
				new FileSynchronizerVisitorFactory(CreateTranscoder()),
				//new EmptyFileSynchronizerVisitorFactory(),
				LogManager.GetLogger(typeof(IFileSynchronizerVisitor))))));

			_container.RegisterType<SynchronizationController>(
				new ContainerControlledLifetimeManager(),
				new InjectionFactory(c => new SynchronizationController(schedulers.ThreadPool)
				));
			_container.RegisterType<ISynchronizationController, SynchronizationController>();
			_container.RegisterType<ISynchronizationNotifications, SynchronizationController>();

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
							schedulers,
							new EmptyNavigationService(),
							new EmptyRequestNavigation(),
							cqrs,
							cqrs,
							new CommandEvents(),
							new NotifyCommandStateBus(cqrs, schedulers.ThreadPool),
							new NotifyQueryStateBus(cqrs)),
							settingsService,
							_container.Resolve<ISynchronizationController>()
						);
					viewModel.Initialize(new NavigationRequest("Main", new Dictionary<string, string>()));
					return viewModel;
				}));
			return _container.Resolve<ConfigurationPageViewModel>();			
		}

		private static IObservable<MusicMirrorConfiguration> GetConfigurationObservable(SettingsService settingsService)
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

		private static IFileTranscoder CreateTranscoder()
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
			return new LoggingFileTranscoder(transcoder, LogManager.GetLogger(typeof(IFileTranscoder)));
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
}
