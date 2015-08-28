using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public sealed class FileInitNotification : FileNotificationBase
	{
		public FileInitNotification(FileInfo fileInfo)
			: base(fileInfo, FileNotificationKind.Initial)
		{
		}

		protected override async Task AcceptInternal(CancellationToken ct, IFileSynchronizerVisitor visitor)
		{
			await visitor.Visit(ct, this);
		}
	}
}