using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public interface IFileNotification
	{
		FileNotificationKind Kind { get; }
		FileInfo FileInfo { get;  }
		Task Accept(CancellationToken ct, IFileSynchronizerVisitor visitor);
	}
}