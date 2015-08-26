using System.Collections.Generic;
using System.IO;

namespace MusicMirror.Tests.Customizations
{
	public class FileInfoEqualityComparer : IEqualityComparer<FileInfo>
	{
		public bool Equals(FileInfo x, FileInfo y)
		{
			if (x == null || y == null)
			{
				return false;
			}
			return x.FullName.Equals(y.FullName);
		}

		public int GetHashCode(FileInfo obj)
		{
			if (obj == null)
			{
				return 0;
			}
			return obj.FullName.GetHashCode();
		}
	}
}