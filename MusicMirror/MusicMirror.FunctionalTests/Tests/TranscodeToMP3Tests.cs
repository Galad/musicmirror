using FluentAssertions;
using MusicMirror.FunctionalTests.Utils;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MusicMirror.FunctionalTests.Tests
{
	[Trait("TestLevel", "Functional")]
	public class TranscodeToMP3Tests : IDisposable, IAsyncLifetime
	{
		private readonly IFixture _fixture;
		private readonly TestContext _context;

		public TranscodeToMP3Tests()
		{
			_fixture = new Fixture().Customize(new SpecificationCustomization());
			_context = _fixture.Create<TestContext>();
		}

		public void Dispose()
		{
			_context.Dispose();
		}

        public async Task InitializeAsync()
        {
            await _context.Load(CancellationToken.None);
        }

        public Task DisposeAsync()
        {
            return Task.FromResult(true);
        }

        [Theory,
			InlineData(TestFilesConstants.MP3.SourceNormalFile1, TestFilesConstants.MP3.NormalFile1),
			InlineData(TestFilesConstants.Flac.SourceNormalFile1, TestFilesConstants.MP3.NormalFile1)
		]
		public async Task WhenITranscodeAFile_ThenTheTranscodedFileShouldBeCreatedInTheTargetFolder(string file, string expectedFileName)
		{
			//arrange
			_context.SourceDirectorySetup()
					.WithFile(file);

			//act
			await _context.ExecuteSynchronization(TestContextUtils.CreateLongTimedOutCancellationToken());
			//assert
			_context.AssertThatTargetFileExistInTargetDirectory(expectedFileName);
		}

		[Theory,
			InlineData(TestFilesConstants.MP3.SourceNormalFile1, TestFilesConstants.MP3.NormalFile1),
			InlineData(TestFilesConstants.Flac.SourceNormalFile1, TestFilesConstants.MP3.NormalFile1)
		]
		public async Task WhenITranscodeAFile_ThenTheTranscodedFileShouldHaveTheCorrectDuration(string file, string expectedFileName)
		{
			//arrange
			_context.SourceDirectorySetup()
					.WithFile(file);

			//act
			await _context.ExecuteSynchronization(TestContextUtils.CreateLongTimedOutCancellationToken());
			//assert
			_context.AssertThatTargetFileHasTheCorrectDuration(Path.GetFileName(file), expectedFileName);
		}

		[Theory,			
			InlineData(TestFilesConstants.Flac.SourceNormalFile1, TestFilesConstants.MP3.NormalFile1)
		]
		public async Task GivenIReadTheFileWithMediaFoundation_WhenITranscodeAFile_ThenTheTranscodedFileShouldHaveTheCorrectDuration(string file, string expectedFileName)
		{
			//arrange
			_context.SourceDirectorySetup()
					.WithFile(file);

			//act
			await _context.ExecuteSynchronization(TestContextUtils.CreateLongTimedOutCancellationToken());
			//assert
			var expectedFileDuration = TestContextUtils.GetFileDurationFlac(_context.GetSourceFileFromRelativePath(Path.GetFileName(file)));
			var actualFileDuration = TestContextUtils.GetMediaFoundationDurationMP3(_context.GetTargetFileFromRelativePath(expectedFileName));
			expectedFileDuration.Should().BeCloseTo(actualFileDuration, 5000);
        }
    }
}
