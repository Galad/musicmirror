using Hanno.Extensions;
using Hanno.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicMirror.ViewModels
{
    public class SynchronizationStatusViewModel : ViewModelBase
    {
        private readonly ISynchronizationController _synchronizationController;
        private readonly ITranscodingNotifications _transcodingNotifications;
        private readonly ILogger _logger;
        private readonly INotificationViewModelProducer _notificationProducer;

        public SynchronizationStatusViewModel(
            IViewModelServices services,
            ISynchronizationController synchronizationController,
            ITranscodingNotifications transcodingNotifications,
            INotificationViewModelProducer notificationProducer,
            ILogger logger) : base(services)
        {
            if (transcodingNotifications == null)
                throw new ArgumentNullException(nameof(transcodingNotifications), $"{nameof(transcodingNotifications)} is null.");
            if (synchronizationController == null) throw new ArgumentNullException(nameof(synchronizationController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _synchronizationController = synchronizationController;
            _transcodingNotifications = transcodingNotifications;            
            _logger = logger;
            _notificationProducer = notificationProducer;
        }

        public IObservableProperty<bool> IsSynchronizationEnabled { get; private set; }
        public IObservableProperty<bool> IsTranscodingRunning { get; private set; }
        public IObservableProperty<SynchronizedFilesCountViewModel> SynchronizedFileCount { get; private set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            IsSynchronizationEnabled = this.GetObservableProperty(() => _synchronizationController.ObserveSynchronizationIsEnabled(), "IsSynchronizationEnabled");
            SynchronizedFileCount = this.GetObservableProperty(_notificationProducer.ObserveSynchronizedFileCount, "GetSynchronizedFileCount");
            IsTranscodingRunning = this.GetObservableProperty(() => _transcodingNotifications.ObserveIsTranscodingRunning().StartWith(Services.Schedulers.Immediate, false), "IsTranscodingRunning");
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
    }
}
