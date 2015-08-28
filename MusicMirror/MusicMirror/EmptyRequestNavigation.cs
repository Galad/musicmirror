using System.Threading;
using System.Threading.Tasks;
using Hanno.Navigation;

namespace MusicMirror
{
	public sealed class EmptyRequestNavigation : IRequestNavigation
	{
		public Task Navigate(CancellationToken ct, INavigationRequest request)
		{
			return Task.FromResult(true);
		}
	}
}