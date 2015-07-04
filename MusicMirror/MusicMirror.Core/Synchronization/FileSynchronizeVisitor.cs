using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace MusicMirror.Synchronization
{
	public class FileSynchronizeVisitor : IFileSynchronizerVisitor
	{
		private readonly IMirroredFolderOperations _synchronizeFile;

		public FileSynchronizeVisitor(IMirroredFolderOperations synchronizeFile)
		{
			if (synchronizeFile == null) throw new ArgumentNullException(nameof(synchronizeFile));
			_synchronizeFile = synchronizeFile;
		}

		public async Task Visit(CancellationToken ct, FileAddedNotification notification)
		{
			await _synchronizeFile.SynchronizeFile(ct, notification.FileInfo);
		}

		public async Task Visit(CancellationToken ct, FileDeletedNotification notification)
		{
			await _synchronizeFile.DeleteFile(ct, notification.FileInfo);
		}

		public async Task Visit(CancellationToken ct, FileModifiedNotification notification)
		{
			await _synchronizeFile.SynchronizeFile(ct, notification.FileInfo);
		}

		public async Task Visit(CancellationToken ct, FileRenamedNotification notification)
		{
			var oldFileInfo = new FileInfo(notification.OldFullPath);
            if (await _synchronizeFile.HasMirroredFileForPath(ct, oldFileInfo))
			{
				await _synchronizeFile.RenameFile(ct, notification.FileInfo, oldFileInfo);
			}
			else
			{
				await _synchronizeFile.SynchronizeFile(ct, notification.FileInfo);
			}
		}

		public async Task Visit(CancellationToken ct, FileInitNotification notification)
		{
			await _synchronizeFile.SynchronizeFile(ct, notification.FileInfo);
		}
	}
}