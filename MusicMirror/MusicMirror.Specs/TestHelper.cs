using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MusicMirror.Specs
{
	public static class TestHelper
	{
		public static void CreateTestFolder(string folderName)
		{
			if (Directory.Exists(GetFolderPath(folderName)))
			{
				Directory.Delete(GetFolderPath(folderName), true);
			}
		}

		public static string GetFolderPath(string folderName)
		{
			return Path.Combine(Path.GetDirectoryName(typeof(TestHelper).Assembly.Location), folderName);
		}

		private static string GetTestFilePath(string fileName)
		{
			return Path.Combine(GetFolderPath("Files"), fileName);
		}

		public static void AddTestFileToFolder(string folderPath, string filename)
		{
			File.Copy(GetTestFilePath(filename), Path.Combine(folderPath, filename));
        }

		public static void AssertFolderContainsFile(string folderPath, string filename)
		{
			var filePath = Path.Combine(folderPath, filename);
			Assert.True(
				File.Exists(filePath),
				string.Format("Expected file {0} to exist but is did not")
				);
		}
	}
}
