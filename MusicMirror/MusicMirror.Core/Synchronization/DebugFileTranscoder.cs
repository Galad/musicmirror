using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror.Synchronization
{
	public class DebugFileTranscoder : IFileTranscoder
	{
		public string GetTranscodedFileName(string sourceFileName)
		{
			return Path.GetFileNameWithoutExtension(sourceFileName) + ".debug";
		}

		public Task Transcode(CancellationToken ct, FileInfo sourceFile, AudioFormat format, DirectoryInfo targetDirectory)
		{
			Debug.WriteLine("Transcoding {0} to {1}", sourceFile.FullName, sourceFile.DirectoryName);
			return Task.FromResult(true);
		}
	}
}
