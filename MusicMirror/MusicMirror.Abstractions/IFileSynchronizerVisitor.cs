using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public interface IFileSynchronizerVisitor
	{
		Task Visit(CancellationToken ct, FileAddedNotification notification);
		Task Visit(CancellationToken ct, FileDeletedNotification notification);
		Task Visit(CancellationToken ct, FileModifiedNotification notification);
		Task Visit(CancellationToken ct, FileRenamedNotification notification);
		Task Visit(CancellationToken ct, FileInitNotification notification);
	}
}