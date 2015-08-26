using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MusicMirror.Synchronization;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Ploeh.AutoFixture;
using Xunit.Extensions;
using Xunit;
using FluentAssertions;
using Moq;
using MusicMirror.Tests.Autofixture;
using static Hanno.Testing.Autofixture.MockExtensions;

namespace MusicMirror.Tests
{
	public class FileSynchronizerTests
	{
		[Theory, AutoMoqData]
		public void Sut_ShouldBeFileSynchronizer(
			FileSynchronizer sut)
		{
			sut.Should().BeAssignableTo<IFileSynchronizer>();
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyGuardClauses(
			GuardClauseAssertion assertion)
		{
			assertion.VerifyType<FileSynchronizer>();
		}

		[Theory, FileAutoData]
		public async Task Synchronize_WhenLastSynchronizationTimeIsOlderThanFileModificationDate_ShouldCallTranscode(
			[Frozen]Mock<ISynchronizedFilesRepository> synchronizedFileRepository,
			[Frozen]Mock<IFileTranscoder> fileTranscoder,
			Configuration config,
			FileSynchronizer sut,
			SourceFilePath sourceFile,
			TargetFilePath targetFile)
		{
			//arrange
			var lastWriteTime = new DateTimeOffset(2015, 04, 01, 0, 0, 0, TimeSpan.Zero);
			var lastSyncTime = lastWriteTime.AddDays(-1);
			sourceFile.LastWriteTime = lastWriteTime;
			synchronizedFileRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), sourceFile.File)).ReturnsTask(targetFile.File);
			synchronizedFileRepository.Setup(s => s.GetLastSynchronization(It.IsAny<CancellationToken>(), sourceFile.File)).ReturnsTask(lastSyncTime);
			//act
			await sut.Synchronize(CancellationToken.None, sourceFile);
			//assert
			fileTranscoder.Verify(f =>
				f.Transcode(
					It.IsAny<CancellationToken>(),
					sourceFile.File,
					AudioFormat.Flac,
                    It.Is((DirectoryInfo d) => d.FullName.Equals(targetFile.File.DirectoryName))));
		}

		[Theory, FileInlineAutoData(0), FileInlineAutoData(1)]
		public async Task Synchronize_WhenLastSynchronizationTimeIsMoreRecentThanFileModificationDate_ShouldNotCallTranscode(
			double daysToAdd,
			[Frozen]Mock<ISynchronizedFilesRepository> synchronizedFileRepository,
			[Frozen]Mock<IFileTranscoder> fileTranscoder,
			Configuration config,
			FileSynchronizer sut,
			SourceFilePath sourceFile,
			TargetFilePath targetFile)
		{
			//arrange
			var lastWriteTime = new DateTimeOffset(2015, 04, 01, 0, 0, 0, TimeSpan.Zero);
			var lastSyncTime = lastWriteTime.AddDays(daysToAdd);
			sourceFile.LastWriteTime = lastWriteTime;
			synchronizedFileRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), sourceFile.File)).ReturnsTask(targetFile.File);
			synchronizedFileRepository.Setup(s => s.GetLastSynchronization(It.IsAny<CancellationToken>(), sourceFile.File)).ReturnsTask(lastSyncTime);
			//act
			await sut.Synchronize(CancellationToken.None, sourceFile);
			//assert
			fileTranscoder.Verify(f =>
				f.Transcode(
					It.IsAny<CancellationToken>(),
					sourceFile.File,
					AudioFormat.Flac,
					It.Is((DirectoryInfo d) => d.FullName.Equals(targetFile.File.DirectoryName))),
				Times.Never());
		}

		[Theory, FileAutoData]
		public async Task Synchronize_WhenFormatIsUknown_ShouldNotCallTranscode(
		[Frozen]Mock<ISynchronizedFilesRepository> synchronizedFileRepository,
		[Frozen]Mock<IFileTranscoder> fileTranscoder,
		Configuration config,
		FileSynchronizer sut,
		TargetFilePath targetFile)
		{
			//arrange
			var expectedExtension = ".uknownExtension";
			var sourceFile = new SourceFilePath(config.SourcePath.FullName, new[] { "test", "test" }, "test" + expectedExtension);
			var lastWriteTime = new DateTimeOffset(2015, 04, 01, 0, 0, 0, TimeSpan.Zero);
			var lastSyncTime = lastWriteTime.AddDays(-1);
			sourceFile.LastWriteTime = lastWriteTime;
			synchronizedFileRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), sourceFile.File)).ReturnsTask(targetFile.File);
			synchronizedFileRepository.Setup(s => s.GetLastSynchronization(It.IsAny<CancellationToken>(), sourceFile.File)).ReturnsTask(lastSyncTime);
			var expectedFormat = new AudioFormat("Uknown format", "Uknown format", expectedExtension, default(LossKind));
			//act
			await sut.Synchronize(CancellationToken.None, sourceFile);
			//assert
			fileTranscoder.Verify(f =>
				f.Transcode(
					It.IsAny<CancellationToken>(),
					sourceFile.File,
					expectedFormat,
					It.Is((DirectoryInfo d) => d.FullName.Equals(targetFile.File.DirectoryName))));
		}
	}
}