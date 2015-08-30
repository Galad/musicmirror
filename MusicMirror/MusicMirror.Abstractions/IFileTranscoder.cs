
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicMirror
{
	/// <summary>
	/// Transcode file in a different format
	/// </summary>
	public interface IFileTranscoder
	{
		/// <summary>
		/// Return the filename which will the result of the transcoding.
		/// </summary>
		/// <param name="sourceFileName">The source file name with the extension. Example : "Take me out.flac"</param>
		/// <returns>The filename with the new extension. Example : "Take me out.mp3"</returns>
		string GetTranscodedFileName(string sourceFileName);

		/// <summary>
		/// Transcode a file
		/// </summary>
		/// <param name="file"></param>
		/// <param name="sourceFile">The file to transcode</param>
		/// <param name="format">The format of the file</param>
		/// <param name="targetDirectory">The directory where the transcoded file will be created.</param>
		/// <exception cref="NotSupportedException">The source file type is not supported. This exception can occurs if the source file extension is not supported, 
		/// or if the source file extension does not correspond to its actual file type, 
		/// or if the source file extension is supported but the encoding format of the source file is not</exception>
		Task Transcode(CancellationToken ct, FileInfo sourceFile, AudioFormat format, DirectoryInfo targetDirectory);
	}	
}