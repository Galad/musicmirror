using MusicMirror.Entities;
using System;
using System.IO;

namespace MusicMirror
{
	public static class FileExtensions
	{
		public static DirectoryInfo GetDirectoryFromSourceFile(this FileSystemInfo sourceFile, Configuration configuration)
		{
			if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			var relativePath = Path.GetDirectoryName(sourceFile.FullName.Replace(configuration.SourcePath.FullName, string.Empty)).TrimStart('\\');
			return new DirectoryInfo(Path.Combine(configuration.TargetPath.FullName, relativePath));
		}
	}
}