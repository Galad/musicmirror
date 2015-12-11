using Hanno;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.ViewModels
{
    public class NotificationViewModelProducer : INotificationViewModelProducer
    {
        private readonly ILogger _logger;
        private readonly ISchedulers _schedulers;
        private readonly ITranscodingNotifications _transcodingNotifications;

        public NotificationViewModelProducer(
            ITranscodingNotifications transcodingNotifications,
            ILogger logger,
            ISchedulers schedulers)
        {
            if (transcodingNotifications == null)
                throw new ArgumentNullException(nameof(transcodingNotifications), $"{nameof(transcodingNotifications)} is null.");            
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulers == null) throw new ArgumentNullException(nameof(schedulers));   
            _transcodingNotifications = transcodingNotifications;
            _logger = logger;
            _schedulers = schedulers;
        }
                
        public IObservable<SynchronizedFilesCountViewModel> ObserveSynchronizedFileCount()
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
                                                      .Replay(5, _schedulers.Immediate);
                                               var disposable = connectableObservable.Connect();
                                               return Observable.Using(() => disposable, _ => connectableObservable);
                                           })
                                           .Switch()
                                           .Where(vm => !vm.IsEmpty)
                                           .DistinctUntilChanged(SynchronizedFilesCountViewModel.StructuralEqualityComparer)
                                           .StartWith(_schedulers.Immediate, SynchronizedFilesCountViewModel.Empty)
                                           .Do(vm => _logger.Info("****Received SynchronizedFileCountNotification. Sucesses : {0}, Total : {1}", vm.SynchronizedFilesCount, vm.TotalFileCount));
        }

        private IObservable<int> ObserveSuccessFileCount()
        {
            return _transcodingNotifications.ObserveTranscodingResult()
                                            .Where(r => r.HandleResult(() => true, _ => false))
                                            .Scan(new IFileTranscodingResultNotification[] { }, (f1, f2) => f1.Concat(new[] { f2 }).ToArray())
                                            .Select(result => result.Length)
                                            .StartWith(_schedulers.Immediate, 0);
        }

        private IObservable<int> ObserveTotalFileCount()
        {
            return _transcodingNotifications.ObserveNotifications()
                                            .Scan(new IFileNotification[] { }, (f1, f2) => f2.Concat(f1).ToArray())
                                            .Select(files => files.Length)
                                            .StartWith(_schedulers.Immediate, 0);
        }

        //public IObservable<ICollectionNotification<FailedTranscodingViewModel>> ObserveFailures()
        //{
        //    return _transcodingNotifications.ObserveTranscodingResult()                                            
        //                                    .Select(r => r.HandleResult(() => null, ex => GetFailureNotification(r.FileNotifications, ex)))
        //                                    .Where(f => f != null);
        //}

        //private ICollectionNotification<FailedTranscodingViewModel> GetFailureNotification(IFileNotification fileNotifications, Exception ex)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
