using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	public abstract class FileNotificationBase : IFileNotification
	{
		protected FileNotificationBase(FileInfo fileInfo, FileNotificationKind kind)
		{
			if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
			FileInfo = fileInfo;
			Kind = kind;
		}

		public FileNotificationKind Kind { get; private set; }
		public FileInfo FileInfo { get; private set; }


		protected abstract Task AcceptInternal(CancellationToken ct, IFileSynchronizerVisitor visitor);

		public Task Accept(CancellationToken ct, IFileSynchronizerVisitor visitor)
		{
			if (visitor == null) throw new ArgumentNullException(nameof(visitor));
			return AcceptInternal(ct, visitor);
		}
	}
}
