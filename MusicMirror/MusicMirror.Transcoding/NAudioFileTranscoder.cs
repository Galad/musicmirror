using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hanno.IO;
using MusicMirror.Synchronization;
using NAudio.Wave;


namespace MusicMirror.Transcoding
{
	public sealed class NAudioFileTranscoder : IFileTranscoder
	{
		private readonly IAsyncFileOperations _asyncFileOperations;
		private readonly IWaveStreamTranscoder _waveStreamTranscoder;
		private readonly IAudioStreamReader _audioStreamReader;
		private readonly IAsyncDirectoryOperations _asyncDirectoryOperations;

		public NAudioFileTranscoder(
			IAudioStreamReader audioStreamReader,
			IWaveStreamTranscoder waveStreamTranscoder,
			IAsyncFileOperations asyncFileOperations,
			IAsyncDirectoryOperations asyncDirectoryOperations)
		{
			if (audioStreamReader == null) throw new ArgumentNullException(nameof(audioStreamReader));			
			if (waveStreamTranscoder == null) throw new ArgumentNullException(nameof(waveStreamTranscoder));
			if (asyncFileOperations == null) throw new ArgumentNullException(nameof(asyncFileOperations));
			if (asyncDirectoryOperations == null) throw new ArgumentNullException(nameof(asyncDirectoryOperations));
			_audioStreamReader = audioStreamReader;			
			_waveStreamTranscoder = waveStreamTranscoder;
			_asyncFileOperations = asyncFileOperations;
			_asyncDirectoryOperations = asyncDirectoryOperations;
		}

		public string GetTranscodedFileName(string sourceFileName)
		{
			if (string.IsNullOrEmpty(sourceFileName)) throw new ArgumentNullException(nameof(sourceFileName));
			return _waveStreamTranscoder.GetTranscodedFileName(sourceFileName);
		}

		public Task Transcode(CancellationToken ct, FileInfo sourceFile, AudioFormat format, DirectoryInfo targetDirectory)
		{
			if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
			if (targetDirectory == null) throw new ArgumentNullException(nameof(targetDirectory));
			if (format == null) throw new ArgumentNullException(nameof(format));
			return TranscodeInternal(ct, sourceFile, format, targetDirectory);
		}

		private async Task TranscodeInternal(CancellationToken ct, FileInfo sourceFile, AudioFormat format, DirectoryInfo targetDirectory)
		{
			if (!await _asyncDirectoryOperations.Exists(targetDirectory.FullName))
			{
				await _asyncDirectoryOperations.CreateDirectory(targetDirectory.FullName);
			}
			var targetFile = Path.Combine(targetDirectory.FullName, GetTranscodedFileName(sourceFile.Name));
			using (var sourceStream = await _asyncFileOperations.OpenRead(sourceFile.FullName))
			using (var sourceWaveStream = await _audioStreamReader.ReadWave(ct, sourceStream, format))
			using (var targetStream = await _asyncFileOperations.OpenWrite(targetFile))
			{
				await _waveStreamTranscoder.Transcode(ct, sourceWaveStream, targetStream);
			}
		}
	}	
}
