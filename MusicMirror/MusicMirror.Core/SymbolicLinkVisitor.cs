using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hanno.IO;
using MusicMirror.Synchronization;

namespace MusicMirror
{
	public sealed class SymbolicLinkFileSynchronizer : IFileSynchronizer
	{
		private readonly IAsyncDirectoryOperations _directoryOperations;
		private readonly IAsyncFileOperations _fileOperations;
		private readonly Configuration _configuration;

		public SymbolicLinkFileSynchronizer(
			Configuration configuration,
			IAsyncFileOperations fileOperations,
			IAsyncDirectoryOperations directoryOperations)
		{
			_configuration = Guard.ForNull(configuration, nameof(configuration));
			_fileOperations = Guard.ForNull(fileOperations, nameof(fileOperations));
			_directoryOperations = Guard.ForNull(directoryOperations, nameof(directoryOperations));
		}

		public async Task Synchronize(CancellationToken ct, IFileInfo sourceFile)
		{
			Guard.ForNull(sourceFile, nameof(sourceFile));
			var targetDirectory = sourceFile.File.GetDirectoryFromSourceFile(_configuration);
			var targetFile = Path.Combine(targetDirectory.FullName, sourceFile.File.Name);
			if (await _fileOperations.Exists(targetFile))
			{
				await _fileOperations.Delete(targetFile);
			}
			else if (!await _directoryOperations.Exists(targetDirectory.FullName))
			{
				await _directoryOperations.CreateDirectory(targetDirectory.FullName);
			}
			SymbolicLink.CreateFileLink(targetFile, sourceFile.File.FullName);
		}
	}
	public sealed class CopyFileSynchronizer : IFileSynchronizer
	{
		private readonly IAsyncDirectoryOperations _directoryOperations;
		private readonly Configuration _configuration;

		public CopyFileSynchronizer(
			Configuration configuration,
			IAsyncDirectoryOperations directoryOperations)
		{
			_configuration = Guard.ForNull(configuration, nameof(configuration));
			_directoryOperations = Guard.ForNull(directoryOperations, nameof(directoryOperations));
		}

		public Task Synchronize(CancellationToken ct, IFileInfo sourceFile)
		{
			Guard.ForNull(sourceFile, nameof(sourceFile));
			return Task.Run(async () =>
			{
				var targetDirectory = sourceFile.File.GetDirectoryFromSourceFile(_configuration);
				var targetFile = Path.Combine(targetDirectory.FullName, sourceFile.File.Name);
				if (!await _directoryOperations.Exists(targetDirectory.FullName))
				{
					await _directoryOperations.CreateDirectory(targetDirectory.FullName);
				}
				if (File.Exists(targetFile))
				{
					var attributes = File.GetAttributes(targetFile);
					File.SetAttributes(targetFile, attributes & ~FileAttributes.ReadOnly);
					//File.Delete(targetFile);
				}
				sourceFile.File.CopyTo(targetFile, true);
			});
		}
	}

	public sealed class SymbolicLinkFileOperations : IAsyncFileOperations
	{
		private readonly IAsyncFileOperations _defaultAsyncFileOperations;

		public SymbolicLinkFileOperations(IAsyncFileOperations defaultAsyncFileOperations)
		{
			this._defaultAsyncFileOperations = Guard.ForNull(defaultAsyncFileOperations, nameof(defaultAsyncFileOperations)); ;
		}
		public async Task Delete(string path)
		{
			await _defaultAsyncFileOperations.Delete(path);
		}

		public async Task<bool> Exists(string path)
		{
			return await _defaultAsyncFileOperations.Exists(path);
		}

		public async Task Move(string sourcePath, string targetPath)
		{
			var symbolicLinkSource = SymbolicLink.GetTarget(sourcePath);
			await _defaultAsyncFileOperations.Delete(sourcePath);
			SymbolicLink.CreateFileLink(targetPath, symbolicLinkSource);
		}

		public async Task<Stream> Open(string path, Hanno.IO.FileMode fileMode, Hanno.IO.FileAccess fileAccess)
		{
			return await _defaultAsyncFileOperations.Open(path, fileMode, fileAccess);
		}

		public async Task<Stream> OpenRead(string path)
		{
			return await _defaultAsyncFileOperations.OpenRead(path);
		}

		public async Task<Stream> OpenWrite(string path)
		{
			return await _defaultAsyncFileOperations.OpenWrite(path);
		}

		public async Task WriteAllBytes(string path, byte[] bytes)
		{
			await _defaultAsyncFileOperations.WriteAllBytes(path, bytes);
		}
	}
}
