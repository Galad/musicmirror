using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public interface ISynchronizedFilesRepository
	{
		Task<FileInfo> GetMirroredFilePath(CancellationToken ct, FileInfo sourceFile);
		Task<DateTimeOffset> GetLastSynchronization(CancellationToken ct, FileInfo sourceFile);
		Task AddSynchronization(CancellationToken ct, FileInfo sourceFile, DateTimeOffset synchronizationTime);
		Task DeleteSynchronization(CancellationToken ct, FileInfo fileInfo);
	}
}