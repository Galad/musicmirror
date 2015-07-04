using System.Threading;
using System.Threading.Tasks;
using Hanno.Navigation;

namespace MusicMirror
{
	public class EmptyRequestNavigation : IRequestNavigation
	{
		public Task Navigate(CancellationToken ct, INavigationRequest request)
		{
			return Task.FromResult(true);
		}
	}
}