using System.Threading;
using MusicMirror.Synchronization;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Ploeh.AutoFixture;
using Xunit.Extensions;
using Xunit;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using System.IO;


namespace MusicMirror.Transcoding.Tests
{
	public class CopyId3TagsPostProcessorTests
	{
		[Theory, FileAutoData]
		public void Sut_ShouldBeIFileTranscoder(
		  CopyId3TagsPostProcessor sut)
		{
			sut.Should().BeAssignableTo<IFileTranscoder>();
		}

		[Theory, FileAutoData]
		public void Sut_VerifyGuardClauses(
		  GuardClauseAssertion assertion)
		{
			assertion.VerifyType<CopyId3TagsPostProcessor>();
		}

		[Theory, FileAutoData]
		public async Task Synchronize_ShouldCallInnerSynchronize(
			[Frozen]Mock<IFileTranscoder> innerFileTranscoder,
			CopyId3TagsPostProcessor sut,
			SourceFilePath sourceFile,
			MusicMirrorConfiguration configuration)
		{
			//arrange
			//act
			await sut.Transcode(CancellationToken.None, sourceFile.File, AudioFormat.Flac, configuration.TargetPath);
			//assert
			innerFileTranscoder.Verify(f => f.Transcode(It.IsAny<CancellationToken>(), sourceFile.File, AudioFormat.Flac, configuration.TargetPath));
		}
		
		[Theory, FileAutoData]
		public async Task Synchronize_ShouldCallAudioTagsSynchronizer(
			[Frozen]Mock<IFileTranscoder> innerFileTranscoder,
			[Frozen]Mock<IAudioTagsSynchronizer> audioTagsSynchronizer,
			CopyId3TagsPostProcessor sut,
			SourceFilePath sourceFile,
			TargetFilePath targetFile)
		{
			//arrange
			innerFileTranscoder.Setup(f => f.GetTranscodedFileName(sourceFile.File.Name)).Returns(targetFile.File.Name);			
			//act
			await sut.Transcode(It.IsAny<CancellationToken>(), sourceFile.File, AudioFormat.Flac, targetFile.File.Directory);
			//assert
			audioTagsSynchronizer.Verify(
				a => a.SynchronizeTags(
					It.IsAny<CancellationToken>(),
					sourceFile.File,
					It.Is((FileInfo f) => new FileInfoEqualityComparer().Equals(f, targetFile.File))));
        }
	}
}