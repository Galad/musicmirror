using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Hanno.Extensions;
using Hanno.Services;
using Hanno.ViewModels;
using System.Reactive.Disposables;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive;
using NLog;

namespace MusicMirror.ViewModels
{
    public sealed class ConfigurationPageViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ISynchronizationController _synchronizationController;
        private readonly ITranscodingNotifications _transcodingNotifications;
        private readonly ILogger _logger;

        public ConfigurationPageViewModel(
            IViewModelServices services,
            ISettingsService settingsService,
            ISynchronizationController synchronizationController,
            ITranscodingNotifications transcodingNotifications,
            ILogger logger) : base(services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services), $"{nameof(services)} is null.");
            if (transcodingNotifications == null)
                throw new ArgumentNullException(nameof(transcodingNotifications), $"{nameof(transcodingNotifications)} is null.");
            if (synchronizationController == null) throw new ArgumentNullException(nameof(synchronizationController));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _settingsService = settingsService;
            _synchronizationController = synchronizationController;
            _transcodingNotifications = transcodingNotifications;
            _logger = logger;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var a = _settingsService.ObserveValue(SettingsConstants.SourcePath, () => string.Empty);
            SourcePath = this.GetObservableProperty(() => _settingsService.ObserveValue(SettingsConstants.SourcePath, () => string.Empty), "SourcePath");
            TargetPath = this.GetObservableProperty(() => _settingsService.ObserveValue(SettingsConstants.TargetPath, () => string.Empty), "TargetPath");
            IsSynchronizationEnabled = this.GetObservableProperty(() => _synchronizationController.ObserveSynchronizationIsEnabled(), "IsSynchronizationEnabled");
            SynchronizedFileCount = this.GetObservableProperty(ObserveSynchronizedFileCount, "GetSynchronizedFileCount");
            IsTranscodingRunning = this.GetObservableProperty(() => _transcodingNotifications.ObserveIsTranscodingRunning().StartWith(Services.Schedulers.Immediate, false), "IsTranscodingRunning");
        }

        public IObservableProperty<string> SourcePath { get; private set; }
        public IObservableProperty<string> TargetPath { get; private set; }
        public IObservableProperty<bool> IsSynchronizationEnabled { get; private set; }
        public IObservableProperty<bool> IsTranscodingRunning { get; private set; }
        public IObservableProperty<SynchronizedFilesCountViewModel> SynchronizedFileCount { get; private set; }

        public ICommand SaveCommand
        {
            get
            {
                return CommandBuilderProvider.Get("SaveCommand")
                    .Execute(async ct => await SaveSettings(ct))
                    .CanExecute(CanExecuteSaveCommand())
                    .ToCommand();
            }
        }

        public ICommand EnableSynchronizationCommand
        {
            get
            {
                return CommandBuilderProvider.Get("EnableSynchronizationCommand")
                                             .Execute(() => _synchronizationController.Enable())
                                             .ToCommand();
            }
        }

        public ICommand DisableSynchronizationCommand
        {
            get
            {
                return CommandBuilderProvider.Get("DisableSynchronizationCommand")
                                             .Execute(() => _synchronizationController.Disable())
                                             .ToCommand();
            }
        }

        private IObservable<SynchronizedFilesCountViewModel> ObserveSynchronizedFileCount()
        {
            return _transcodingNotifications.ObserveIsTranscodingRunning()
                                            .Where(b => b)                                            
                                            .Select(o =>
                                            {
                                                var compositeDisposable = new CompositeDisposable();
                                                //var o1 = ObserveTotalFileCount().ReplayAndConnect(5, compositeDisposable, Services.Schedulers.Immediate);
                                                //var o2 = ObserveSuccessFileCount().ReplayAndConnect(5, compositeDisposable, Services.Schedulers.Immediate);
                                                var o1 = ObserveTotalFileCount();
                                                var o2 = ObserveSuccessFileCount();
                                                var connectableObservable = Observable.CombineLatest(
                                                       o1,
                                                       o2,
                                                       (total, successes) =>
                                                       {                                                           
                                                           return new SynchronizedFilesCountViewModel(successes, total);
                                                       })
                                                       .Do(vm => _logger.Info("Received SynchronizedFileCountNotification. Sucesses : {0}, Total : {1}", vm.SynchronizedFilesCount, vm.TotalFileCount))                                                       
                                                       .TakeUntil(_transcodingNotifications.ObserveIsTranscodingRunning().Where(b => !b))
                                                       .Replay(5, Services.Schedulers.Immediate);
                                                var disposable = connectableObservable.Connect();
                                                return Observable.Using(() => disposable, _ => connectableObservable);                                               
                                            })
                                            .Switch()
                                            .Where(vm => !vm.IsEmpty)
                                            .StartWith(Services.Schedulers.Immediate, SynchronizedFilesCountViewModel.Empty);
        }

        private IObservable<SynchronizedFilesCountViewModel> ObserveEmptyFilesCount()
        {
            return _transcodingNotifications.ObserveIsTranscodingRunning()
                                            .StartWith(Services.Schedulers.Immediate, false)
                                            .DistinctUntilChanged()
                                            .Where(b => !b)
                                            .Select(_ => SynchronizedFilesCountViewModel.Empty)
                                            .SubscribeOn(Services.Schedulers.Immediate);
        }

        private IObservable<int> ObserveSuccessFileCount()
        {
            return _transcodingNotifications.ObserveTranscodingResult()                                            
                                            .Where(r => r.HandleResult(() => true, _ => false))
                                            .Scan(new IFileTranscodingResultNotification[] { }, (f1, f2) => f1.Concat(new[] { f2 }).ToArray())                                            
                                            .Select(result => result.Length)                                            
                                            .StartWith(Services.Schedulers.Immediate, 0);
        }

        private IObservable<int> ObserveTotalFileCount()
        {
            return _transcodingNotifications.ObserveNotifications()
                                            .Scan(new IFileNotification[] { }, (f1, f2) => f2.Concat(f1).ToArray())
                                            .Select(files => files.Length)
                                            .StartWith(Services.Schedulers.Immediate, 0);
        }

        private async Task SaveSettings(CancellationToken ct)
        {
            await _settingsService.SetValue("SourcePath", SourcePath.Value, ct);
            await _settingsService.SetValue("TargetPath", TargetPath.Value, ct);
        }

        private IObservable<bool> CanExecuteSaveCommand()
        {
            return Observable.CombineLatest(SourcePath, TargetPath)
                             .Select(paths => paths.All(path => !string.IsNullOrEmpty(path)));
        }
    }
}
