using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MusicMirror.Synchronization;

namespace MusicMirror.Logging
{
	public sealed class LoggingFileTranscoder : IFileTranscoder
	{
		private readonly ILog _log;
		private readonly IFileTranscoder _fileTranscoder;

		public LoggingFileTranscoder(IFileTranscoder fileTranscoder, ILog log)
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
            _log.InfoFormat("Transcoding file {0} to {1}", sourceFileName, targetFileName);
			try
			{
				await _fileTranscoder.Transcode(ct, sourceFile, format, targetDirectory);
				_log.InfoFormat("Transcoding complete for file", sourceFileName, targetFileName);
			}
			catch (Exception ex)
			{
				_log.Error("Error while logging file " + sourceFile, ex);
				throw;
			}			
		}
	}
}
