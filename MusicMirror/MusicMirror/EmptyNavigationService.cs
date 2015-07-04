using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hanno.Navigation;

namespace MusicMirror
{
	public class EmptyNavigationService : INavigationService
	{
		public Task NavigateBack(CancellationToken ct)
		{
			return Task.FromResult(true);
		}

		public bool CanNavigateBack { get { return false; }}
		public INavigationHistory History { get { return new NavigationHistory((token, entry) => Task.FromResult(true), token => Task.FromResult(true)); } }
		public IObservable<INavigationRequest> Navigating { get { return Observable.Empty<INavigationRequest>(); } }
		public IObservable<INavigationRequest> Navigated { get { return Observable.Empty<INavigationRequest>(); } }
	}
}
