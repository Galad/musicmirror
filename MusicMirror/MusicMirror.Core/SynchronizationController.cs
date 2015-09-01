using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
	public sealed class SynchronizationController : ISynchronizationController, ISynchronizationNotifications, IDisposable
	{
		private readonly IScheduler _scheduler;
		private readonly ReplaySubject<bool> _isEnabled;

		public IScheduler Scheduler
		{
			get
			{
				return _scheduler;
			}
		}

		public SynchronizationController(IScheduler scheduler)
		{
			if (scheduler == null)throw new ArgumentNullException(nameof(scheduler));
			_scheduler = scheduler;
			_isEnabled = new ReplaySubject<bool>(1, _scheduler);
			_isEnabled.OnNext(false);
		}

		public IObservable<IObservable<FileSynchronizationResult>> ObserveSynchronizationNotifications()
		{
			return Observable.Return(Observable.Empty<FileSynchronizationResult>());
			//return Observable.Empty<IObservable<FileSynchronizationResult>>();
            //return Observable.Never<IObservable<FileSynchronizationResult>>();
        }

		public void Enable()
		{
			_isEnabled.OnNext(true);			
		}

		public void Disable()
		{
			_isEnabled.OnNext(false);
		}

		public IObservable<bool> ObserveSynchronizationIsEnabled()
		{
			return _isEnabled.AsObservable();
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
					_isEnabled.Dispose();
				}

				// TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
				// TODO: définir les champs de grande taille avec la valeur Null.

				disposedValue = true;
			}
		}

		// TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
		// ~SynchronizationController() {
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
