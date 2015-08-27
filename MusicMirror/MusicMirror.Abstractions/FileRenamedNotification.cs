using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public class FileRenamedNotification : FileNotificationBase
	{
		public string OldFullPath { get; private set; }

		public FileRenamedNotification(FileInfo fileInfo, string oldFullPath)
			: base(fileInfo, FileNotificationKind.Renamed)
		{
			if (string.IsNullOrEmpty(oldFullPath)) throw new ArgumentNullException(nameof(oldFullPath));
			OldFullPath = oldFullPath;
		}

		protected override async Task AcceptInternal(CancellationToken ct, IFileSynchronizerVisitor visitor)
		{
			await visitor.Visit(ct, this);
		}
	}
}