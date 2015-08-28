using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public sealed class ChooseFolderOperation : IMirroredFolderOperations
	{
		private readonly IMirroredFolderOperations _defaultFileOperations;
		private readonly IMirroredFolderOperations _transcodingFolderOperations;
		private readonly IRequireTranscoding _requireTranscoding;		

		public IMirroredFolderOperations TranscodingFolderOperation { get { return _transcodingFolderOperations; } }

		public IMirroredFolderOperations DefaultFileOperations { get { return _defaultFileOperations; } }

		public ChooseFolderOperation(
			IRequireTranscoding requireTranscoding,
			IMirroredFolderOperations defaultFileOperations,
			IMirroredFolderOperations transcodingFolderOperation)
		{
			_requireTranscoding = Guard.ForNull(requireTranscoding, nameof(requireTranscoding));
			_defaultFileOperations = Guard.ForNull(defaultFileOperations, nameof(defaultFileOperations));
			_transcodingFolderOperations = Guard.ForNull(transcodingFolderOperation, nameof(transcodingFolderOperation));
		}

		public Task DeleteFile(CancellationToken ct, FileInfo file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			return DeleteFileInternal(ct, file);
		}

		private async Task DeleteFileInternal(CancellationToken ct, FileInfo file)
		{
			var folderOperations = await GetMirroredFolderOperations(ct, file);
			await folderOperations.DeleteFile(ct, file);
		}

		public Task<bool> HasMirroredFileForPath(CancellationToken ct, FileInfo file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			return HasMirroredFileForPathInternal(ct, file);
		}

		private async Task<bool> HasMirroredFileForPathInternal(CancellationToken ct, FileInfo file)
		{
			var folderOperations = await GetMirroredFolderOperations(ct, file);
			return await folderOperations.HasMirroredFileForPath(ct, file);
		}

		public Task RenameFile(CancellationToken ct, FileInfo newFile, FileInfo oldFile)
		{
			if (newFile == null) throw new ArgumentNullException(nameof(newFile));
			if (oldFile == null) throw new ArgumentNullException(nameof(oldFile));
			return RenameFileInternal(ct, newFile, oldFile);
		}

		private async Task RenameFileInternal(CancellationToken ct, FileInfo newFile, FileInfo oldFile)
		{
			var oldFileRequireTranscoding = await _requireTranscoding.ForFile(ct, oldFile);
			var newFileRequireTranscoding = await _requireTranscoding.ForFile(ct, newFile);
			if (oldFileRequireTranscoding && newFileRequireTranscoding)
			{
				await _transcodingFolderOperations.RenameFile(ct, newFile, oldFile);
			}
			else if (newFileRequireTranscoding)
			{
				await _transcodingFolderOperations.SynchronizeFile(ct, newFile);
				await _defaultFileOperations.DeleteFile(ct, oldFile);
			}
			else if (oldFileRequireTranscoding)
			{
				await _defaultFileOperations.SynchronizeFile(ct, newFile);
				await _transcodingFolderOperations.DeleteFile(ct, oldFile);
			}
			else
			{
				await _defaultFileOperations.RenameFile(ct, newFile, oldFile);
			}
		}

		public Task SynchronizeFile(CancellationToken ct, FileInfo file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			return SynchronizeFileInternal(ct, file);
		}

		private async Task SynchronizeFileInternal(CancellationToken ct, FileInfo file)
		{
			var folderOperations = await GetMirroredFolderOperations(ct, file);
			await folderOperations.SynchronizeFile(ct, file);
		}

		private async Task<IMirroredFolderOperations> GetMirroredFolderOperations(CancellationToken ct, FileInfo file)
		{
			return await _requireTranscoding.ForFile(ct, file) ? _transcodingFolderOperations : _defaultFileOperations;
        }
	}
}
