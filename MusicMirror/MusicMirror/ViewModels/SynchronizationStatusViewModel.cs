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

        public SynchronizationStatusViewModel(
            IViewModelServices services,
            ISynchronizationController synchronizationController,
            ITranscodingNotifications transcodingNotifications,
            ILogger logger) : base(services)
        {
            if (transcodingNotifications == null)
                throw new ArgumentNullException(nameof(transcodingNotifications), $"{nameof(transcodingNotifications)} is null.");
            if (synchronizationController == null) throw new ArgumentNullException(nameof(synchronizationController));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _synchronizationController = synchronizationController;
            _transcodingNotifications = transcodingNotifications;
            _logger = logger;            
        }

        public IObservableProperty<bool> IsSynchronizationEnabled { get; private set; }
        public IObservableProperty<bool> IsTranscodingRunning { get; private set; }
        public IObservableProperty<SynchronizedFilesCountViewModel> SynchronizedFileCount { get; private set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            IsSynchronizationEnabled = this.GetObservableProperty(() => _synchronizationController.ObserveSynchronizationIsEnabled(), "IsSynchronizationEnabled");
            SynchronizedFileCount = this.GetObservableProperty(ObserveSynchronizedFileCount, "GetSynchronizedFileCount");
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

        private IObservable<SynchronizedFilesCountViewModel> ObserveSynchronizedFileCount()
        {
            return _transcodingNotifications.ObserveIsTranscodingRunning()
                                            .Where(b => b)
                                            .Select(o =>
                                            {
                                                var compositeDisposable = new CompositeDisposable();
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
                                            .DistinctUntilChanged(SynchronizedFilesCountViewModel.StructuralEqualityComparer)
                                            .StartWith(Services.Schedulers.Immediate, SynchronizedFilesCountViewModel.Empty)
                                            .Do(vm => _logger.Info("****Received SynchronizedFileCountNotification. Sucesses : {0}, Total : {1}", vm.SynchronizedFilesCount, vm.TotalFileCount));
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
