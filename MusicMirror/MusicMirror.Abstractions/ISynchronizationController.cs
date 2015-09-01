﻿using System;
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
		IObservable<IObservable<FileSynchronizationResult>> ObserveSynchronizationNotifications();
	}
}