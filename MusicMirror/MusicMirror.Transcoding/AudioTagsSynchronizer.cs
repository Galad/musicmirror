using Hanno.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Transcoding
{
	public sealed class AudioTagsSynchronizer : IAudioTagsSynchronizer
	{
		private readonly IAsyncFileOperations _asyncFileOperations;
		private readonly IAudioTagReader _audioTagReader;
		private readonly IAudioTagWriter _audioTagWriter;

		public AudioTagsSynchronizer(
			IAsyncFileOperations asyncFileOperations,
			IAudioTagReader audioTagReader,
			IAudioTagWriter audioTagWriter)
		{
			_asyncFileOperations = Guard.ForNull(asyncFileOperations, nameof(asyncFileOperations));
			_audioTagReader = Guard.ForNull(audioTagReader, nameof(audioTagReader));
			_audioTagWriter = Guard.ForNull(audioTagWriter, nameof(audioTagWriter));
		}

		public Task SynchronizeTags(CancellationToken ct, FileInfo sourceFile, FileInfo targetFile)
		{
			Guard.ForNull(sourceFile, nameof(sourceFile));
			Guard.ForNull(targetFile, nameof(targetFile));
			return SynchronizeTagsInternal(ct, sourceFile, targetFile);
		}

		private async Task SynchronizeTagsInternal(CancellationToken ct, FileInfo sourceFile, FileInfo targetFile)
		{
			using (var sourceStream = await _asyncFileOperations.OpenRead(sourceFile.FullName))
			using (var targetStream = await _asyncFileOperations.Open(targetFile.FullName, Hanno.IO.FileMode.Open, Hanno.IO.FileAccess.ReadWrite))
			{
				var tags = await _audioTagReader.ReadTags(ct, sourceStream);
				await _audioTagWriter.WriteTags(ct, targetStream, tags);
			}
		}
	}
}
