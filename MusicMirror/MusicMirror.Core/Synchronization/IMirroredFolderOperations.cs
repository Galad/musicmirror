using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public interface IMirroredFolderOperations
	{
		Task RenameFile(CancellationToken ct, FileInfo newFile, FileInfo oldFile);
		Task DeleteFile(CancellationToken ct, FileInfo file);
		Task SynchronizeFile(CancellationToken ct, FileInfo file);
		Task<bool> HasMirroredFileForPath(CancellationToken ct, FileInfo fullPath);
	}
}