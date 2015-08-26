using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public class SynchronizeSymbolicLinkFile : IMirroredFolderOperations
	{
		public Task DeleteFile(CancellationToken ct, FileInfo file)
		{
			throw new NotImplementedException();
		}

		public Task<bool> HasMirroredFileForPath(CancellationToken ct, FileInfo file)
		{
			throw new NotImplementedException();
		}

		public Task RenameFile(CancellationToken ct, FileInfo newFile, FileInfo oldFile)
		{
			throw new NotImplementedException();
		}

		public Task SynchronizeFile(CancellationToken ct, FileInfo file)
		{
			throw new NotImplementedException();
		}
	}
}