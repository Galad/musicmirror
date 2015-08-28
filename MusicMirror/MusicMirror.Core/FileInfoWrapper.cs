using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
	sealed class FileInfoWrapper : IFileInfo
	{
		private readonly FileInfo _file;

		public FileInfoWrapper(FileInfo file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			_file = file;
		}

		public DateTimeOffset LastWriteTime { get { return new DateTimeOffset(_file.LastWriteTimeUtc); } }
		public FileInfo File { get { return _file; } }

		public bool IsReadOnly
		{
			get
			{
				return _file.IsReadOnly;
			}
		}
	}
}
