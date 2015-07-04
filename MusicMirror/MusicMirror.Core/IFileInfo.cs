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

	class FileInfoWrapper : IFileInfo
	{
		private readonly FileInfo _file;

		public FileInfoWrapper(FileInfo file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			_file = file;
		}

		public DateTimeOffset LastWriteTime { get { return new DateTimeOffset(_file.LastWriteTimeUtc); } }
		public FileInfo File { get { return _file; } }

		public bool IsReadonly
		{
			get
			{
				return _file.IsReadOnly;
			}
		}
	}
}