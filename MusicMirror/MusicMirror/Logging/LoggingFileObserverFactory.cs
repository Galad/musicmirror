using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.Logging
{
    public sealed class LoggingFileObserverFactory : IFileObserverFactory
    {
        private readonly IFileObserverFactory _innerFileObserverFactory;
        private readonly ILogger _logger;

        public LoggingFileObserverFactory(IFileObserverFactory innerFileObserverFactory, ILogger logger)
        {
            if (innerFileObserverFactory == null) throw new ArgumentNullException("innerFileObserverFactory");
            if (logger == null) throw new ArgumentNullException("logger");
            _innerFileObserverFactory = innerFileObserverFactory;
            _logger = logger;
        }

        public IObservable<IFileNotification[]> GetFileObserver(DirectoryInfo path)
        {
            return new LoggingFileObserver(_innerFileObserverFactory.GetFileObserver(path), _logger);
        }

        private class LoggingFileObserver : IObservable<IFileNotification[]>
        {
            private readonly ILogger _logger;
            private IObservable<IFileNotification[]> _observable;

            public LoggingFileObserver(IObservable<IFileNotification[]> observable, ILogger logger)
            {
                if (observable == null) throw new ArgumentNullException("observable");
                if (logger == null) throw new ArgumentNullException("logger");
                _observable = observable;
                _logger = logger;
            }

            public IDisposable Subscribe(IObserver<IFileNotification[]> observer)
            {
                return _observable.Do(files =>
                            _logger.Info(() => files.Length + " file notifications found"),
                            ex => _logger.Error(ex))
                    .Subscribe(observer);
                                
            }
        }
    }
}
