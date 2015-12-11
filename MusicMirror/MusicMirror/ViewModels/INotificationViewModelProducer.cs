using System;

namespace MusicMirror.ViewModels
{
    public interface INotificationViewModelProducer
    {
        IObservable<SynchronizedFilesCountViewModel> ObserveSynchronizedFileCount();
        //IObservable<ICollectionNotification<FailedTranscodingViewModel>> ObserveFailures();
    }
}