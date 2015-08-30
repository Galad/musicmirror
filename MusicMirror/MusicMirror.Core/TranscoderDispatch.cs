using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MusicMirror.Synchronization;


namespace MusicMirror
{
	public sealed class TranscoderDispatch : IFileTranscoder
	{
		private readonly IFileTranscoder _defaultTranscoder;
		private readonly List<TranscoderEntry> _transcoders;

		sealed private class TranscoderEntry
		{
			public AudioFormat Format;
			public IFileTranscoder Transcoder;
		}
		public TranscoderDispatch(IFileTranscoder defaultTranscoder)
		{
			if (defaultTranscoder == null) throw new ArgumentNullException(nameof(defaultTranscoder));
			_defaultTranscoder = defaultTranscoder;
			_transcoders = new List<TranscoderEntry>();
		}

		public void AddTranscoder(IFileTranscoder transcoder, params AudioFormat[] formats)
		{
			if (transcoder == null) throw new ArgumentNullException(nameof(transcoder));
			if (formats == null) throw new ArgumentNullException(nameof(formats));
			foreach (var format in formats)
			{
				_transcoders.Add(new TranscoderEntry() { Format = format, Transcoder = transcoder });
			}
		}

		public string GetTranscodedFileName(string sourceFileName)
		{
			if (string.IsNullOrEmpty(sourceFileName)) throw new ArgumentNullException(nameof(sourceFileName));
			var transcoder = GetTranscoderForExtension(Path.GetExtension(sourceFileName));
			return transcoder.GetTranscodedFileName(sourceFileName);
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
			var transcoder = GetTranscoderForExtension(sourceFile.Extension);
			await transcoder.Transcode(ct, sourceFile, format, targetDirectory);
		}

		private IFileTranscoder GetTranscoderForExtension(string extension)
		{
			var transcoder = _transcoders.FirstOrDefault(t => t.Format.SupportExtension(extension));			
			if (transcoder == null)
			{
				return _defaultTranscoder;
			}
			return transcoder.Transcoder;
		}
	}
}