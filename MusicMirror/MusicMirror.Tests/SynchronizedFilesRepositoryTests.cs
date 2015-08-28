using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Hanno;
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
using MusicMirror.Entities;

namespace MusicMirror.Tests
{
	public class SynchronizedFilesRepositoryTests
	{
		[Theory, AutoMoqData]
		public void Sut_ShouldBeISynchronizedFilesRepository(
		  SynchronizedFilesRepository sut)
		{
			sut.Should().BeAssignableTo<ISynchronizedFilesRepository>();
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyGuardClauses(
		  GuardClauseAssertion assertion)
		{
			assertion.VerifyType<SynchronizedFilesRepository>();
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyConstructorInitializedMember(
		  ConstructorInitializedMemberAssertion assertion)
		{
			assertion.VerifyType<SynchronizedFilesRepository>();
		}

		[Theory, FileAutoData]
		public async Task GetMirroredFilePath_ShouldReturnCorrectValue(
			[Frozen]Mock<IFileTranscoder> fileTranscoder,
			MusicMirrorConfiguration configuration,
			SynchronizedFilesRepository sut,
			SourceFilePath fileInfo,
			TargetFilePath expectedFile)
		{
			//arrange
			fileTranscoder.Setup(f => f.GetTranscodedFileName(fileInfo.File.Name)).Returns(expectedFile.File.Name);
			var expected = new FileInfo(Path.Combine(configuration.TargetPath.FullName, fileInfo.RelativePath, expectedFile.File.Name));
			//act
			var actual = await sut.GetMirroredFilePath(CancellationToken.None, fileInfo.File);

			//assert
			Assert.Equal(expected, actual, new FileInfoEqualityComparer());
		}

		[Theory, FileAutoData]
		public async Task GetLastSynchronization_WhenAddingSynchronization_ShouldReturnCorrectValue(
			SynchronizedFilesRepository sut,
			FileInfo file,
			DateTimeOffset expected)
		{
			//arrange
			await sut.AddSynchronization(CancellationToken.None, file, expected);
			//act
			var actual = await sut.GetLastSynchronization(CancellationToken.None, file);
			//assert
			Assert.Equal(expected, actual);
		}

		[Theory, FileAutoData]
		public async Task GetLastSynchronization_WhenRepositoryIsEmpty_ShouldReturnCorrectValue(
			SynchronizedFilesRepository sut,
			FileInfo file)
		{
			//arrange
			var expected = DateTimeOffset.MinValue;
			//act
			var actual = await sut.GetLastSynchronization(CancellationToken.None, file);
			//assert
			Assert.Equal(expected, actual);
		}

		[Theory, FileAutoData]
		public async Task DeleteSynchronization_WhenAddingSynchronizationThenRemoving_ShouldReturnCorrectValue(
			SynchronizedFilesRepository sut,
			FileInfo file,
			DateTimeOffset time)
		{
			//arrange
			var expected = DateTimeOffset.MinValue;
			await sut.AddSynchronization(CancellationToken.None, file, time);
			await sut.DeleteSynchronization(CancellationToken.None, file);
			//act
			var actual = await sut.GetLastSynchronization(CancellationToken.None, file);
			//assert
			Assert.Equal(expected, actual);
		}
	}
}