using System.Linq;


namespace MusicMirror.Tests.Customizations
{
	/// <summary>
	/// Represent a file path from the source folder
	/// </summary>
	public class SourceFilePath : FilePathBase
	{
		public SourceFilePath(string basePath, string[] relativePath, string filename) 
			: base(basePath, relativePath, filename)
		{
		}

		public static SourceFilePath CreateFromPathWithoutExtension(
			MusicMirrorConfiguration config, 
			string[] relativePathParts)
		{
			return new SourceFilePath(
				config.SourcePath.FullName,
				relativePathParts.Skip(1).ToArray(),
				relativePathParts[0] + ".flac");
		}
	}
}