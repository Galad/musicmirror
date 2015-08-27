using System;
using System.IO;

namespace MusicMirror
{
	/// <summary>
	/// Provides informations about a file
	/// </summary>
	public interface IFileInfo
	{
		/// <summary>
		/// Returns the last write time for the file, in UTC format
		/// </summary>
		DateTimeOffset LastWriteTime { get; }
		/// <summary>
		/// The source <c>FileInfo</c>
		/// </summary>
		FileInfo File { get; }
		/// <summary>
		/// Indicates wether the file is read only
		/// </summary>
		bool IsReadonly { get; }
	}	
}