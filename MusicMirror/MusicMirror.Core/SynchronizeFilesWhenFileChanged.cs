using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;

namespace MusicMirror
{
    public sealed class SynchronizeFilesWhenFileChanged : IStartSynchronizing, ITranscodingNotifications, IDisposable
    {
        private readonly IObservable<MusicMirrorConfiguration> _configurationObservable;
        private readonly IFileObserverFactory _fileObserverFactory;
        private readonly IFileSynchronizerVisitorFactory _fileSynchronizerVisitorFactory;
        private readonly Subject<IFileTranscodingResultNotification> _transcodingResultNotifications;
        private readonly Subject<IFileNotification[]> _fileNotifications;
        private readonly IObservable<bool> _isTranscodingRunning;
        private readonly CompositeDisposable _subscribtions;
        private IScheduler _synchronizationScheduler;
        private readonly Subject<int> _numberOfFilesAddedInTranscodingQueue;
        private readonly IScheduler _notificationsScheduler;
        private readonly Subject<Unit> _restartListeningToNotifications;

        public IObservable<MusicMirrorConfiguration> ConfigurationObservable => _configurationObservable;
        public IFileObserverFactory FileObserverFactory => _fileObserverFactory;
        public IFileSynchronizerVisitorFactory FileSynchronizerVisitorFactory => _fileSynchronizerVisitorFactory;

        public IScheduler SynchronizationScheduler
        {
            get
            {
                return _synchronizationScheduler;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(SynchronizationScheduler));
                _synchronizationScheduler = value;
            }
        }

        public IScheduler NotificationsScheduler => _notificationsScheduler;

        public SynchronizeFilesWhenFileChanged(
            IObservable<MusicMirrorConfiguration> configurationObservable,
            IFileObserverFactory fileObserverFactory,
            IFileSynchronizerVisitorFactory fileSynchronizerVisitorFactory,
            IScheduler synchronizationScheduler,
            IScheduler notificationsScheduler)
        {
            if (configurationObservable == null) throw new ArgumentNullException(nameof(configurationObservable));
            if (fileObserverFactory == null) throw new ArgumentNullException(nameof(fileObserverFactory));
            if (fileSynchronizerVisitorFactory == null) throw new ArgumentNullException(nameof(fileSynchronizerVisitorFactory));
            if (synchronizationScheduler == null) throw new ArgumentNullException(nameof(synchronizationScheduler));
            if (notificationsScheduler == null) throw new ArgumentNullException(nameof(notificationsScheduler));            
            _subscribtions = new CompositeDisposable();
            _configurationObservable = configurationObservable;
            _fileObserverFactory = fileObserverFactory;
            _fileSynchronizerVisitorFactory = fileSynchronizerVisitorFactory;
            _transcodingResultNotifications = new Subject<IFileTranscodingResultNotification>();
            _fileNotifications = new Subject<IFileNotification[]>();
            _restartListeningToNotifications = new Subject<Unit>();
            //_numberOfFilesAddedInTranscodingQueue = new ReplaySubject<int>(1, ImmediateScheduler.Instance).DisposeWith(_subscribtions);
            //_numberOfFilesAddedInTranscodingQueue.OnNext(0);
            _numberOfFilesAddedInTranscodingQueue = new Subject<int>();
            _isTranscodingRunning = ObserveIsTranscodinningRunningCold().ReplayAndConnect(1, _subscribtions, ImmediateScheduler.Instance);
            _numberOfFilesAddedInTranscodingQueue.OnNext(0);
            _synchronizationScheduler = synchronizationScheduler;
            _notificationsScheduler = notificationsScheduler;
        }

        public IDisposable Subscribe()
        {
            return ConfigurationObservable
                       .Select(ObserveFiles)
                       .Switch()
                       .Subscribe();
        }

        private IObservable<Unit> ObserveFiles(MusicMirrorConfiguration configuration)
        {
            var visitor = FileSynchronizerVisitorFactory.CreateVisitor(configuration);
            return FileObserverFactory.GetFileObserver(configuration.SourcePath)
                                      .ObserveOn(_notificationsScheduler)
                                      .Do(files =>
                                      {
                                          _numberOfFilesAddedInTranscodingQueue.OnNext(files.Length);
                                          _fileNotifications.OnNext(files);
                                      })
                                      .SelectMany(files => files.Select(file => SynchronizeFile(file, visitor))
                                                                .Merge(4, _synchronizationScheduler)
                                                                .ToList()
                                                                .SelectUnit());
        }

        private IObservable<Unit> SynchronizeFile(IFileNotification file, IFileSynchronizerVisitor visitor)
        {
            return Observable.FromAsync(async ct => await file.Accept(ct, visitor), _notificationsScheduler)                             
                             .Do(_ =>
                             {
                                 _transcodingResultNotifications.OnNext(FileTranscodingResultNotification.CreateSuccess(file));
                                 _numberOfFilesAddedInTranscodingQueue.OnNext(-1);
                             })
                             .Catch((Exception ex) =>
                             {
                                 _transcodingResultNotifications.OnNext(FileTranscodingResultNotification.CreateFailure(file, ex));
                                 _numberOfFilesAddedInTranscodingQueue.OnNext(-1);
                                 return Observable.Return(Unit.Default, ImmediateScheduler.Instance);
                             });
        }

        public IObservable<IFileNotification[]> ObserveNotifications()
        {
            return _fileNotifications.AsObservable();
        }

        public IObservable<IFileTranscodingResultNotification> ObserveTranscodingResult()
        {
            return _transcodingResultNotifications.AsObservable();
        }

        public IDisposable Start()
        {
            return new CompositeDisposable(
                Subscribe(),
                Disposable.Create(() =>
                {
                    _restartListeningToNotifications.OnNext(Unit.Default);
                    _numberOfFilesAddedInTranscodingQueue.OnNext(0);
                })
                );
        }

        public IObservable<bool> ObserveIsTranscodingRunning()
        {
            return _isTranscodingRunning;
        }

        private IObservable<bool> ObserveIsTranscodinningRunningCold()
        {
            return _restartListeningToNotifications
                .StartWith(ImmediateScheduler.Instance, Unit.Default)
                .Select(_ => _numberOfFilesAddedInTranscodingQueue.Scan(0, (x, y) => x + y))
                .Switch()
                .Select(filesInQueue => filesInQueue > 0)
                .DistinctUntilChanged();
        }

        #region IDisposable Support
        private bool disposedValue = false; // Pour détecter les appels redondants

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés).
                    _subscribtions.Dispose();
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }

        // TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
        // ~SynchronizeFilesWhenFileChanged() {
        //   // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
        //   Dispose(false);
        // }

        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose()
        {
            // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
            Dispose(true);
            // TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}