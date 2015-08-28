using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hanno;
using Hanno.IO;
using MusicMirror.Entities;

namespace MusicMirror.Synchronization
{
	public sealed class SynchronizeFileService : IMirroredFolderOperations
	{
		private readonly IAsyncFileOperations _asyncFileOperations;
		private readonly MusicMirrorConfiguration _configuration;
		private readonly IFileSynchronizer _fileSynchronizer;
		private readonly ISynchronizedFilesRepository _synchronizedFilesRepository;

		public SynchronizeFileService(
			IAsyncFileOperations asyncFileOperations,
			MusicMirrorConfiguration configuration,
			IFileSynchronizer fileSynchronizer,
			ISynchronizedFilesRepository synchronizedFilesRepository)
		{
			if (asyncFileOperations == null) throw new ArgumentNullException(nameof(asyncFileOperations));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			if (fileSynchronizer == null) throw new ArgumentNullException(nameof(fileSynchronizer));
			if (synchronizedFilesRepository == null) throw new ArgumentNullException(nameof(synchronizedFilesRepository));
			_asyncFileOperations = asyncFileOperations;
			_configuration = configuration;
			_fileSynchronizer = fileSynchronizer;
			_synchronizedFilesRepository = synchronizedFilesRepository;
		}

		public IAsyncFileOperations AsyncFileOperations { get { return _asyncFileOperations; } }
		public MusicMirrorConfiguration Configuration { get { return _configuration; } }
		public IFileSynchronizer FileSynchronizer { get { return _fileSynchronizer; } }
		public ISynchronizedFilesRepository SynchronizedFilesRepository { get { return _synchronizedFilesRepository; } }

		public Task RenameFile(CancellationToken ct, FileInfo newFile, FileInfo oldFile)
		{
			if (newFile == null) throw new ArgumentNullException(nameof(newFile));
			if (oldFile == null) throw new ArgumentNullException(nameof(oldFile));
			return RenameFileInternal(ct, newFile, oldFile);
		}

		private async Task RenameFileInternal(CancellationToken ct, FileInfo newFile, FileInfo oldFile)
		{
			var sourceFile = await _synchronizedFilesRepository.GetMirroredFilePath(ct, oldFile);
			var targetFile = await _synchronizedFilesRepository.GetMirroredFilePath(ct, newFile);
			await _asyncFileOperations.Move(sourceFile.FullName, targetFile.FullName);
			await _synchronizedFilesRepository.AddSynchronization(ct, newFile, NowContext.Current.Now);
			await _synchronizedFilesRepository.DeleteSynchronization(ct, oldFile);
		}

		public Task DeleteFile(CancellationToken ct, FileInfo file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			return DeleteFileInternal(ct, file);
		}

		private async Task DeleteFileInternal(CancellationToken ct, FileInfo file)
		{
			var targetFile = await _synchronizedFilesRepository.GetMirroredFilePath(ct, file);
			await _asyncFileOperations.Delete(targetFile.FullName);
			await _synchronizedFilesRepository.DeleteSynchronization(ct, file);
		}

		public Task SynchronizeFile(CancellationToken ct, FileInfo file)
		{
			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}
			return SynchronizeFileInternal(ct, file);
		}

		private async Task SynchronizeFileInternal(CancellationToken ct, FileInfo file)
		{
			var mirroredFile = await _synchronizedFilesRepository.GetMirroredFilePath(ct, file);
			await _fileSynchronizer.Synchronize(ct, new FileInfoWrapper(file));
			await _synchronizedFilesRepository.AddSynchronization(ct, file, NowContext.Current.Now);
		}

		public Task<bool> HasMirroredFileForPath(CancellationToken ct, FileInfo file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			return HasMirroredFileForPathInternal(ct, file);
		}

		private async Task<bool> HasMirroredFileForPathInternal(CancellationToken ct, FileInfo file)
		{
			var mirroredFile = await _synchronizedFilesRepository.GetMirroredFilePath(ct, file);
			return await _asyncFileOperations.Exists(mirroredFile.FullName);
		}
	}
}