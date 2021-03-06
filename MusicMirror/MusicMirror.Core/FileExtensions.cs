﻿
using System;
using System.IO;

namespace MusicMirror
{
	public static class FileExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Function applies to files only")]
		public static DirectoryInfo GetDirectoryFromSourceFile(this FileInfo sourceFile, MusicMirrorConfiguration configuration)
		{
			if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			var relativePath = Path.GetDirectoryName(sourceFile.FullName.Replace(configuration.SourcePath.FullName, string.Empty)).TrimStart('\\');
			return new DirectoryInfo(Path.Combine(configuration.TargetPath.FullName, relativePath));
		}
	}
}