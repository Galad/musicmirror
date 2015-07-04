using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit2;
using Xunit.Extensions;
using FluentAssertions;
using Moq;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture.Idioms;
using System.Threading;
using Hanno.IO;
using System.IO;
using TagLib;
using Hanno.Testing.Autofixture;
using Xunit;

namespace MusicMirror.Transcoding.Tests
{
	public class AudioTagsSynchronizerTests
	{

		[Theory, FileAutoData]
		public void Sut_ShouldBeIAudioTagsSynchronizer(
			AudioTagsSynchronizer sut)
		{
			sut.Should().BeAssignableTo<IAudioTagsSynchronizer>();
		}
		
		[Theory, FileAutoData]
		public void Sut_VerifyGuardClauses(
			GuardClauseAssertion assertion)
		{
			assertion.VerifyType<AudioTagsSynchronizer>();
        }

		[Theory, FileAutoData]
		public async Task Synchronize_ShouldCallSynchronize(
			[Frozen]Mock<IAsyncFileOperations> fileOperations,
			[Frozen]Mock<IAudioTagReader> audioTagReader,
			[Frozen]Mock<IAudioTagWriter> audioTagWriter,
			AudioTagsSynchronizer sut,
			SourceFilePath sourceFile,
			TargetFilePath targetFile,
			Stream sourceStream,
			Stream targetStream,
            Tag tag)
		{
			//arrange
			fileOperations.Setup(f => f.OpenRead(sourceFile.ToString())).ReturnsTask(sourceStream);
			fileOperations.Setup(f => f.Open(targetFile.ToString(), Hanno.IO.FileMode.Open, Hanno.IO.FileAccess.ReadWrite)).ReturnsTask(targetStream);
			audioTagReader.Setup(a => a.ReadTags(It.IsAny<CancellationToken>(), sourceStream)).ReturnsTask(tag);
			//act
			await sut.SynchronizeTags(CancellationToken.None, sourceFile.File, targetFile.File);
			//assert
			audioTagWriter.Verify(a => a.WriteTags(It.IsAny<CancellationToken>(), targetStream, tag));
        }
	}
}
