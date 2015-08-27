using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public class FileAddedNotification : FileNotificationBase
	{
		public FileAddedNotification(FileInfo fileInfo)
			:base(fileInfo, FileNotificationKind.Added)
		{
		}

		protected override async Task AcceptInternal(CancellationToken ct, IFileSynchronizerVisitor visitor)
		{
			await visitor.Visit(ct, this);
		}
	}
}