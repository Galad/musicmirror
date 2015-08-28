using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hanno.IO;
using Moq;
using MusicMirror.Synchronization;
using MusicMirror.Tests.Autofixture;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using static Hanno.Testing.Autofixture.MockExtensions;
using Ploeh.AutoFixture;
using MusicMirror.Entities;

namespace MusicMirror.Transcoding.Tests
{
	public class NAudioFileTranscoderTests
	{
		[Theory, AutoMoqData]
		public void Sut_ShouldBeIFileTranscoder(
		 NAudioFileTranscoder sut)
		{
			sut.Should().BeAssignableTo<IFileTranscoder>();
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyGuardClauses(
		 GuardClauseAssertion assertion)
		{
			assertion.VerifyType<NAudioFileTranscoder>();
		}

		[Theory, FileAutoData]
		public async Task Transcode_ShouldCallWaveTranscoder(
			 [Frozen]Mock<IWaveStreamTranscoder> waveStreamTranscoder,
			 [Frozen]Mock<IAudioStreamReader> audioStreamReader,
			 [Frozen]Mock<IAsyncFileOperations> asyncFileOperations,
			 NAudioFileTranscoder sut,
			 MusicMirrorConfiguration config,
			 SourceFilePath sourceFile,
			 TargetFilePath targetFile,
			 IWaveStreamProvider waveStream,
			 Stream sourceStream,
			 Stream targetStream)
		{
			//arrange
			waveStreamTranscoder.Setup(t => t.GetTranscodedFileName(sourceFile.File.Name)).Returns(targetFile.File.Name);
			asyncFileOperations.Setup(f => f.OpenRead(sourceFile.ToString())).ReturnsTask(sourceStream);
			asyncFileOperations.Setup(f => f.OpenWrite(Path.Combine(config.TargetPath.FullName, targetFile.File.Name))).ReturnsTask(targetStream);
			audioStreamReader.Setup(a => a.ReadWave(It.IsAny<CancellationToken>(), sourceStream, AudioFormat.FLAC))
				.ReturnsTask(waveStream);
			//act
			await sut.Transcode(CancellationToken.None, sourceFile.File, AudioFormat.FLAC, config.TargetPath);
			//assert
			waveStreamTranscoder.Verify(t => t.Transcode(It.IsAny<CancellationToken>(), waveStream, targetStream));
		}
		
		[Theory,
			FileInlineAutoData(true, false),
			FileInlineAutoData(false, true),
			]
		public async Task Transcode_WhenDirectorDoesNotExist_ShouldCreateDirectory(
			 bool exist,
			 bool shouldCallCreateDirectory,
			 [Frozen]Mock<IAsyncDirectoryOperations> asyncDirectoryOperations,
			 NAudioFileTranscoder sut,
			 MusicMirrorConfiguration config,
			 SourceFilePath sourceFile,
			 IFixture fixture)
		{			
			//arrange
			asyncDirectoryOperations.Setup(d => d.Exists(config.TargetPath.FullName)).ReturnsTask(exist);
			//act
			await sut.Transcode(CancellationToken.None, sourceFile.File, AudioFormat.FLAC, config.TargetPath);
			//assetr
			asyncDirectoryOperations.VerifyOnce(d => d.CreateDirectory(config.TargetPath.FullName), shouldCallCreateDirectory);
		}
	}

	public static class MockExtensions
	{
		public static void VerifyOnce<T>(this Mock<T> mock, Expression<Action<T>> expression, bool shouldHaveBeenCalled)
			where T : class
		{
			mock.Verify(expression, shouldHaveBeenCalled ? Times.Once() : Times.Never());
		}
	}
}
