using MusicMirror.Entities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public class SynchronizedFilesRepository : ISynchronizedFilesRepository
	{
		private readonly IFileTranscoder _fileTranscoder;
		private readonly Configuration _configuration;
		private readonly ConcurrentDictionary<string, DateTimeOffset> _synchronizations;

		public SynchronizedFilesRepository(
			IFileTranscoder fileTranscoder,
			Configuration configuration)
		{
			if (fileTranscoder == null) throw new ArgumentNullException(nameof(fileTranscoder));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			_fileTranscoder = fileTranscoder;
			_configuration = configuration;
			_synchronizations = new ConcurrentDictionary<string, DateTimeOffset>();
		}

		public IFileTranscoder FileTranscoder { get { return _fileTranscoder; } }

		public Configuration Configuration { get { return _configuration; } }

		public Task AddSynchronization(CancellationToken ct, FileInfo sourceFile, DateTimeOffset synchronizationTime)
		{
			if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
			_synchronizations.AddOrUpdate(sourceFile.FullName, synchronizationTime, (s, offset) => synchronizationTime);
			return Task.FromResult(true);
		}

		public Task DeleteSynchronization(CancellationToken ct, FileInfo fileInfo)
		{
			if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
			DateTimeOffset time;
			_synchronizations.TryRemove(fileInfo.FullName, out time);
			return Task.FromResult(true);
		}

		public Task<DateTimeOffset> GetLastSynchronization(CancellationToken ct, FileInfo sourceFile)
		{
			if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
			DateTimeOffset time;
			if (!_synchronizations.TryGetValue(sourceFile.FullName, out time))
			{
				return Task.FromResult(DateTimeOffset.MinValue);
			}
			return Task.FromResult(time);
		}

		public Task<FileInfo> GetMirroredFilePath(CancellationToken ct, FileInfo sourceFile)
		{
			if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
			var fileName = _fileTranscoder.GetTranscodedFileName(sourceFile.Name);			
			var mirroredFile = new FileInfo(Path.Combine(sourceFile.GetDirectoryFromSourceFile(_configuration).FullName, fileName));			
			return Task.FromResult(mirroredFile);
		}
	}
}