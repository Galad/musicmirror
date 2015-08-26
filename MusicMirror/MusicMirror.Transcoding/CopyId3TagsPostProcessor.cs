using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MusicMirror.Synchronization;
using NAudio.Flac;

namespace MusicMirror.Transcoding
{
	public class CopyId3TagsPostProcessor : IFileTranscoder
	{
		private readonly IFileTranscoder _innerTranscoder;
		private readonly IAudioTagsSynchronizer _audioTagsSynchronizer;
				
		public CopyId3TagsPostProcessor(
			IFileTranscoder innerTranscoder,
			IAudioTagsSynchronizer audioTagsSynchronizer)
		{			
			_innerTranscoder = Guard.ForNull(innerTranscoder, nameof(innerTranscoder));
			_audioTagsSynchronizer = Guard.ForNull(audioTagsSynchronizer, nameof(audioTagsSynchronizer));
		}
		public string GetTranscodedFileName(string sourceFileName)
		{
			Guard.ForNullOrWhiteSpace(sourceFileName, nameof(sourceFileName));
			return _innerTranscoder.GetTranscodedFileName(sourceFileName);
		}

		public Task Transcode(CancellationToken ct, FileInfo sourceFile, AudioFormat format, DirectoryInfo targetDirectory)
		{
			Guard.ForNull(sourceFile, nameof(sourceFile));
			Guard.ForNull(format, nameof(format));
			Guard.ForNull(targetDirectory, nameof(targetDirectory));
			return TranscodeInternal(ct, sourceFile, format, targetDirectory);
		}

		private async Task TranscodeInternal(CancellationToken ct, FileInfo sourceFile, AudioFormat format, DirectoryInfo targetDirectory)
		{
			await _innerTranscoder.Transcode(ct, sourceFile, format, targetDirectory);
			await _audioTagsSynchronizer.SynchronizeTags(ct, sourceFile, new FileInfo(Path.Combine(targetDirectory.FullName, GetTranscodedFileName(sourceFile.Name))));
        }
	}	
}
