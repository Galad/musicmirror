using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MusicMirror.Synchronization;
using NLog;

namespace MusicMirror.Logging
{
	public sealed class LoggingFileTranscoder : IFileTranscoder
	{
		private readonly ILogger _log;
		private readonly IFileTranscoder _fileTranscoder;

		public LoggingFileTranscoder(IFileTranscoder fileTranscoder, ILogger log)
		{
			_fileTranscoder = Guard.ForNull(fileTranscoder, nameof(fileTranscoder));
			_log = Guard.ForNull(log, nameof(log));
		}

		public string GetTranscodedFileName(string sourceFileName)
		{
			return _fileTranscoder.GetTranscodedFileName(sourceFileName);
		}

		public async Task Transcode(CancellationToken ct, FileInfo sourceFile, AudioFormat format, DirectoryInfo targetDirectory)
		{
			var sourceFileName = sourceFile.FullName;
			var targetFileName = GetTranscodedFileName(sourceFile.FullName);
            _log.Info("Transcoding file {0} to {1}", sourceFileName, targetFileName);
			try
			{
				await _fileTranscoder.Transcode(ct, sourceFile, format, targetDirectory);
				_log.Info("Transcoding complete for file {0} {1}", sourceFileName, targetFileName);
			}
			catch (Exception ex)
			{
				_log.Error(ex, "Error while logging file {0}", sourceFileName);
				throw;
			}			
		}
	}
}
