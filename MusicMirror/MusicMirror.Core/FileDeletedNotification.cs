using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public class FileDeletedNotification : FileNotificationBase
	{
		public FileDeletedNotification(FileInfo fileInfo)
			: base(fileInfo, FileNotificationKind.Deleted)
		{
		}

		protected override async Task AcceptInternal(CancellationToken ct, IFileSynchronizerVisitor visitor)
		{			
			await visitor.Visit(ct, this);
		}
	}
}