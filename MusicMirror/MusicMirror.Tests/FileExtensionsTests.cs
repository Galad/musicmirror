using System.IO;
using System.Reflection;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Ploeh.AutoFixture;
using Xunit.Extensions;
using Xunit;
using FluentAssertions;
using Moq;
using MusicMirror.Entities;
using System.Security.Permissions;

namespace MusicMirror.Tests
{
	public class FileExtensionsTests
	{
		[Theory, FileAutoData]
		public void FileExtensions_VerifyGuardClauses(
		  GuardClauseAssertion assertion)
		{			
			var methods = typeof (FileExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static);
			assertion.Verify(methods);
		}

		[Theory, FileAutoData]
		public void GetDirectoryFromSourceFile_ShouldReturnCorrectValue(
			SourceFilePath sourceFile,
			MusicMirrorConfiguration configuration)
		{
			//arrange
			var expected = Path.Combine(configuration.TargetPath.FullName, sourceFile.RelativePath);
			//act
			var actual = sourceFile.File.GetDirectoryFromSourceFile(configuration);
			//assert
			actual.FullName.Should().Be(expected);
		}
	}
}