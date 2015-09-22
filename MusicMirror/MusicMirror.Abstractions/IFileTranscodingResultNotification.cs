using System;

namespace MusicMirror
{
    public interface IFileTranscodingResultNotification
    {
        IFileNotification FileNotifications { get; }
        T HandleResult<T>(Func<T> success, Func<Exception, T> failure);
    }

    public static class FileTranscodingResultNotificationExtensions
    {
        public static void HandleResult(this IFileTranscodingResultNotification notification, Action success, Action<Exception> failure)
        {
            if (failure == null)
                throw new ArgumentNullException(nameof(failure), $"{nameof(failure)} is null.");
            if (success == null)
                throw new ArgumentNullException(nameof(success), $"{nameof(success)} is null.");
            if (notification == null)
                throw new ArgumentNullException(nameof(notification), $"{nameof(notification)} is null.");
            Func<bool> successFunc = () =>
            {
                success();
                return true;
            };
            Func<Exception, bool> failureFunc = (ex) =>
            {
                failure(ex);
                return true;
            };
            notification.HandleResult<bool>(successFunc, failureFunc);
        }
    }
}