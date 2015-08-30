using FluentAssertions;
using MusicMirror.FunctionalTests.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MusicMirror.FunctionalTests.Assertion
{
	[Trait("TestLevel", "Functional")]
	public class AssertionsTests
	{
		[Fact]
		public void GetFileDurationMP3_ShouldReturnCorrectValue()
		{
			//arrange
			var file = Path.Combine(System.Environment.CurrentDirectory, SpecificationCustomization.ReferenceTestFileRootFolder, TestFilesConstants.MP3.SourceNormalFile1);
			var expected = TimeSpan.FromSeconds(4 * 60 + 18);
			//act
			var actual = TestContextExtensions.GetFileDurationMP3(file);
			//assert
			actual.Should().BeCloseTo(expected, 1000);
		}

		[Fact]
		public void GetFileDurationFlac_ShouldReturnCorrectValue()
		{
			//arrange
			var file = Path.Combine(System.Environment.CurrentDirectory, SpecificationCustomization.ReferenceTestFileRootFolder, TestFilesConstants.Flac.SourceNormalFile1);
			var expected = TimeSpan.FromSeconds(7 * 60 + 15);
			//act
			var actual = TestContextExtensions.GetFileDurationFlac(file);
			//assert
			actual.Should().BeCloseTo(expected, 1000);
		}

		[Fact]
		public void GetEstimatedDurationFlac_ShouldReturnCorrectValue()
		{
			//arrange
			var file = Path.Combine(System.Environment.CurrentDirectory, SpecificationCustomization.ReferenceTestFileRootFolder, TestFilesConstants.MP3.SourceFileWithWrongDisplayedDuration);
			var expected = TimeSpan.FromSeconds(7 * 60 + 49);
			//act
			var actual = TestContextExtensions.GetMediaFoundationDurationMP3(file);
			//assert
			actual.Should().BeCloseTo(expected, 1000);
		}
	}
}
