using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
	public interface ISynchronizationController
	{
		void Enable();
		void Disable();
		IObservable<bool> ObserveSynchronizationIsEnabled();
	}

	public interface ISynchronizationNotifications
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IObservable<IObservable<FileSynchronizationResult>> ObserveSynchronizationNotifications();
	}
}
