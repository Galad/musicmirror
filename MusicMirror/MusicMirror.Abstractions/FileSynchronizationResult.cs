using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
    public abstract class FileTranscodingResultNotification
    {
        private readonly IFileNotification _fileNotifications;

        public FileTranscodingResultNotification(IFileNotification fileNotifications)
        {
            if (fileNotifications == null)
                throw new ArgumentNullException(nameof(fileNotifications), $"{nameof(fileNotifications)} is null.");
            _fileNotifications = fileNotifications;
        }

        public IFileNotification FileNotifications
        {
            get
            {
                return _fileNotifications;
            }
        }

        public static IFileTranscodingResultNotification CreateSuccess(IFileNotification fileNotifications)
        {
            return new SuccessTranscodingResultNotification(fileNotifications);
        }

        public static IFileTranscodingResultNotification CreateFailure(IFileNotification fileNotifications, Exception exception)
        {
            return new FailureTranscodingResultNotification(fileNotifications, exception);
        }

        public class SuccessTranscodingResultNotification : FileTranscodingResultNotification, IFileTranscodingResultNotification
        {
            public SuccessTranscodingResultNotification(IFileNotification fileNotifications) : base(fileNotifications)
            {
            }

            public T HandleResult<T>(Func<T> success, Func<Exception, T> failure)
            {
                if (success == null)
                    throw new ArgumentNullException(nameof(success), $"{nameof(success)} is null.");
                if (failure == null)
                    throw new ArgumentNullException(nameof(failure), $"{nameof(failure)} is null.");
                return success();
            }
        }

        public class FailureTranscodingResultNotification : FileTranscodingResultNotification, IFileTranscodingResultNotification
        {
            private readonly Exception _exception;

            public FailureTranscodingResultNotification(IFileNotification fileNotifications, Exception exception) : base(fileNotifications)
            {
                if (exception == null)
                    throw new ArgumentNullException(nameof(exception), $"{nameof(exception)} is null.");
                _exception = exception;
            }

            public Exception Exception { get { return _exception; } }

            public T HandleResult<T>(Func<T> success, Func<Exception, T> failure)
            {
                if (success == null)
                    throw new ArgumentNullException(nameof(success), $"{nameof(success)} is null.");
                if (failure == null)
                    throw new ArgumentNullException(nameof(failure), $"{nameof(failure)} is null.");
                return failure(Exception);
            }
        }
    }
}
