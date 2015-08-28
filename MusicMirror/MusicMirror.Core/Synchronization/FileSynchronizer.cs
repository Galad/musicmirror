using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public sealed class FileSynchronizer : IFileSynchronizer
	{
		private readonly ISynchronizedFilesRepository _synchronizedFileRepository;
		private readonly IFileTranscoder _transcoder;

		public FileSynchronizer(
			ISynchronizedFilesRepository synchronizedFileRepository,
			IFileTranscoder transcoder)
		{
			if (synchronizedFileRepository == null) throw new ArgumentNullException(nameof(synchronizedFileRepository));
			if (transcoder == null) throw new ArgumentNullException(nameof(transcoder));
			_synchronizedFileRepository = synchronizedFileRepository;
			_transcoder = transcoder;
		}

		public Task Synchronize(CancellationToken ct, IFileInfo sourceFile)
		{
			if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
			return SynchronizeInternal(ct, sourceFile);
		}

		private async Task SynchronizeInternal(CancellationToken ct, IFileInfo sourceFile)
		{
			var lastSync = await _synchronizedFileRepository.GetLastSynchronization(ct, sourceFile.File);
			if (lastSync >= sourceFile.LastWriteTime)
			{
				return;
			}
			var mirroredFile = await _synchronizedFileRepository.GetMirroredFilePath(ct, sourceFile.File);
			var sourceFileFormat = AudioFormat.KnownFormats.FirstOrDefault(
				f => f.AllExtensions.Any(
					ext => ext.Equals(
						sourceFile.File.Extension,
						StringComparison.OrdinalIgnoreCase)))
						?? new AudioFormat("Uknown format", "Uknown format", sourceFile.File.Extension, default(LossKind));
			await _transcoder.Transcode(ct, sourceFile.File, sourceFileFormat, mirroredFile.Directory);
		}
	}
}