using System;
using System.Linq;
using MusicMirror.Entities;

namespace MusicMirror.Tests.Customizations
{
	/// <summary>
	/// Represent a file path from the target folder
	/// </summary>
	public class TargetFilePath : FilePathBase
	{
		public TargetFilePath(string basePath, string[] relativePath, string filename) 
			: base(basePath, relativePath, filename)
		{
		}

		public static TargetFilePath CreateFromPathWithoutExtension(
			MusicMirrorConfiguration config,
			string[] relativePathParts)
		{
			return new TargetFilePath(
				config.TargetPath.FullName,
				relativePathParts.Skip(1).ToArray(),
				relativePathParts[0] + ".mp3");
		}
	}
}