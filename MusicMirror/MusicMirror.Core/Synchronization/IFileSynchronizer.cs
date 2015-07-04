using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public interface IFileSynchronizer
	{
		Task Synchronize(CancellationToken ct, IFileInfo sourceFile);
	}
}