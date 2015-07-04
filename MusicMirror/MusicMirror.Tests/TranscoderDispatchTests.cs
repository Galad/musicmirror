using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MusicMirror.Synchronization;
using MusicMirror.Tests.Autofixture;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace MusicMirror.Tests
{
	public class TranscoderDispatchTests
	{
		[Theory, AutoMoqData]
		public void Sut_ShouldBeITranscoder(
		  TranscoderDispatch sut)
		{
			sut.Should().BeAssignableTo<IFileTranscoder>();
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyGuardClauses(
		  GuardClauseAssertion assertion)
		{
			assertion.VerifyType<TranscoderDispatch>();
		}

		[Theory, AutoMoqData]
		public void GetTranscodedFileName_ShouldReturnCorrectValue(
			TranscoderDispatch sut,
			string extension,
			string filename,
			string expected,
			Mock<IFileTranscoder> fileTranscoder)
		{
			//arrange
			extension = new string(extension.Take(3).ToArray());
			var sourceFile = String.Concat(filename, ".", extension);
			fileTranscoder.Setup(f => f.GetTranscodedFileName(sourceFile)).Returns(expected);
			sut.AddTranscoder(fileTranscoder.Object, new AudioFormat("test", "test", "." + extension, LossKind.Lossless));
			//act
			var actual = sut.GetTranscodedFileName(sourceFile);
			//assert
			actual.Should().Be(expected);
		}

		[Theory, AutoMoqData]
		public void GetTranscodedFileName_WhenNoThereIsNoTranscoder_ShouldReturnCorrectValue(
			[Frozen]Mock<IFileTranscoder> fileTranscoder,
			TranscoderDispatch sut,
			string filename,
			string expected
			)
		{
			//arrange			
			fileTranscoder.Setup(f => f.GetTranscodedFileName(filename)).Returns(expected);
			//act
			var actual = sut.GetTranscodedFileName(filename);
			//assert
			actual.Should().Be(expected);
		}

		[Theory, FileAutoData]
		public async Task Transcode_ShouldCallTranscoder(
			TranscoderDispatch sut,
			SourceFilePath file,
			DirectoryInfo directory,
			Mock<IFileTranscoder> fileTranscoder)
		{
			//arrange
			sut.AddTranscoder(fileTranscoder.Object, new AudioFormat("test", "test", file.File.Extension, LossKind.Lossless));
			//act
			await sut.Transcode(CancellationToken.None, file.File, AudioFormat.Flac, directory);
			//assert
			fileTranscoder.Verify(f => f.Transcode(CancellationToken.None, file.File, AudioFormat.Flac, directory));
		}

		[Theory, FileAutoData]
		public async Task Transcode_WhenNoThereIsNoTranscoder_ShouldCallDefaultTranscoder(
			[Frozen]Mock<IFileTranscoder> fileTranscoder,
			TranscoderDispatch sut,
			SourceFilePath file,
			DirectoryInfo directory)
		{
			//arrange
			//act
			await sut.Transcode(CancellationToken.None, file.File, AudioFormat.Flac, directory);
			//assert
			fileTranscoder.Verify(f => f.Transcode(CancellationToken.None, file.File, AudioFormat.Flac, directory));
		}
	}
}