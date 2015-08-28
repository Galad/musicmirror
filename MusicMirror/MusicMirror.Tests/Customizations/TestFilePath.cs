using System;
using System.Collections.Generic;
using System.IO;

namespace MusicMirror.Tests.Customizations
{
	public abstract class FilePathBase : IFileInfo
	{
		private readonly FileInfo _fileInfo;
		private readonly IEnumerable<string> _relativePath;
		private DateTimeOffset? _lastWriteTime;

		protected FilePathBase(string basePath, string[] relativePath, string filename)
		{
			_fileInfo = new FileInfo(Path.Combine(basePath, string.Join("\\", relativePath), filename));
			_relativePath = relativePath;
		}

		public DateTimeOffset LastWriteTime
		{
			get
			{
				if (_lastWriteTime.HasValue)
				{
					return _lastWriteTime.Value;
				}
				return this.File.LastWriteTime;
			}
			set { _lastWriteTime = value; }
		}

		public FileInfo File { get { return _fileInfo; } }
		public string RelativePath { get { return string.Join("\\", _relativePath); } }

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public override string ToString()
		{
			return _fileInfo.FullName;
		}
	}
}