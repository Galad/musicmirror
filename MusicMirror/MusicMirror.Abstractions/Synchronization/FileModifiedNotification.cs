using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public sealed class FileModifiedNotification : FileNotificationBase
	{
		public FileModifiedNotification(FileInfo fileInfo)
			: base(fileInfo, FileNotificationKind.Modified)
		{
		}

		protected override async Task AcceptInternal(CancellationToken ct, IFileSynchronizerVisitor visitor)
		{
			await visitor.Visit(ct, this);
		}
	}
}