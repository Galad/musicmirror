using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hanno.Testing.Autofixture;
using Moq;
using MusicMirror.Synchronization;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace MusicMirror.Tests.Synchronization
{
	public class ChooseFolderOperationTests
	{
		[Theory, FileAutoData]
		public void Sut_ShouldBeIFolderOperations(
			ChooseFolderOperation sut)
		{
			sut.Should().BeAssignableTo<IMirroredFolderOperations>();
		}
		
		[Theory, FileAutoData]
		public void Sut_VerifyGuardClauses(
			GuardClauseAssertion assertion)
		{ 
			assertion.VerifyType<ChooseFolderOperation>();
		}
		
		[Theory, FileAutoData]
		public void Sut_VerifyConstructorInitialized(
			ConstructorInitializedMemberAssertion assertion)
		{
			assertion.VerifyType<IMirroredFolderOperations>();
		}

		[Theory, FileAutoData]
		public async Task DeleteFile_WhenFileRequireTranscoding_ShouldCallTranscodingFolderOperations(			
			[Frozen]Mock<IRequireTranscoding> requireTranscoding,
			ChooseFolderOperation sut,
			SourceFilePath file)
		{
			//arrange			
			var transcodingFolderOperation = Mock.Get(sut.TranscodingFolderOperation);			
			var verifiable = transcodingFolderOperation.Setup(m => m.DeleteFile(It.IsAny<CancellationToken>(), file.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(true);
			//act
			await sut.DeleteFile(CancellationToken.None, file.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileAutoData]
		public async Task DeleteFile_WhenFileDoesNotRequireTranscoding_ShouldCallDefaultFolderOperations(
			[Frozen]Mock<IRequireTranscoding> requireTranscoding,
			ChooseFolderOperation sut,
			SourceFilePath file)
		{
			//arrange
			var defaultFolderOperation = Mock.Get(sut.DefaultFileOperations);
			var verifiable = defaultFolderOperation.Setup(m => m.DeleteFile(It.IsAny<CancellationToken>(), file.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			//act
			await sut.DeleteFile(CancellationToken.None, file.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileInlineAutoData(true), FileInlineAutoData(false)]
		public async Task HasMirroredFileForPath_WhenFileRequireTranscoding_ShouldReturnCorrectValue(
			bool expected,
			[Frozen]Mock<IRequireTranscoding> requireTranscoding,
			ChooseFolderOperation sut,
			SourceFilePath file)
		{
			//arrange			
			var transcodingFolderOperation = Mock.Get(sut.TranscodingFolderOperation);
			transcodingFolderOperation.Setup(m => m.HasMirroredFileForPath(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(expected);			
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(true);
			//act
			var actual = await sut.HasMirroredFileForPath(CancellationToken.None, file.File);
			//assert
			actual.Should().Be(expected);
		}

		[Theory, FileInlineAutoData(true), FileInlineAutoData(false)]
		public async Task HasMirroredFileForPath_WhenFileDoesNotRequireTranscoding_ShouldReturnCorrectValue(
			bool expected,
			[Frozen]Mock<IRequireTranscoding> requireTranscoding,
			ChooseFolderOperation sut,
			SourceFilePath file)
		{
			//arrange
			var defaultFolderOperation = Mock.Get(sut.DefaultFileOperations);
			defaultFolderOperation.Setup(m => m.HasMirroredFileForPath(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(expected);
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			//act
			var actual = await sut.HasMirroredFileForPath(CancellationToken.None, file.File);
			//assert
			actual.Should().Be(expected);
		}

		[Theory, FileAutoData]
		public async Task SynchronizeFile_WhenFileRequireTranscoding_ShouldCallTranscodingFolderOperations(
			[Frozen]Mock<IRequireTranscoding> requireTranscoding,
			ChooseFolderOperation sut,
			SourceFilePath file)
		{
			//arrange			
			var transcodingFolderOperation = Mock.Get(sut.TranscodingFolderOperation);
			var verifiable = transcodingFolderOperation.Setup(m => m.SynchronizeFile(It.IsAny<CancellationToken>(), file.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(true);
			//act
			await sut.SynchronizeFile(CancellationToken.None, file.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileAutoData]
		public async Task SynchronizeFile_WhenFileDoesNotRequireTranscoding_ShouldCallDefaultFolderOperations(
			[Frozen]Mock<IRequireTranscoding> requireTranscoding,
			ChooseFolderOperation sut,
			SourceFilePath file)
		{
			//arrange
			var defaultFolderOperation = Mock.Get(sut.DefaultFileOperations);
			var verifiable = defaultFolderOperation.Setup(m => m.SynchronizeFile(It.IsAny<CancellationToken>(), file.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			//act
			await sut.SynchronizeFile(CancellationToken.None, file.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileAutoData]
		public async Task Rename_ShouldCallTranscodingFolderOperations(
			[Frozen]Mock<IRequireTranscoding> requireTranscoding,
			ChooseFolderOperation sut,
			SourceFilePath newFile,
			SourceFilePath oldFile)
		{
			//arrange			
			var transcodingFolderOperation = Mock.Get(sut.TranscodingFolderOperation);
			var verifiable = transcodingFolderOperation.Setup(m => m.RenameFile(It.IsAny<CancellationToken>(), newFile.File, oldFile.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), newFile.File)).ReturnsTask(true);
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), oldFile.File)).ReturnsTask(true);
			//act
			await sut.RenameFile(CancellationToken.None, newFile.File, oldFile.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileAutoData]
		public async Task Rename_ShouldCallDefaultFolderOperations(
			[Frozen]Mock<IRequireTranscoding> requireTranscoding,
			ChooseFolderOperation sut,
			SourceFilePath newFile,
			SourceFilePath oldFile)
		{
			//arrange			
			var defaultFolderOperation = Mock.Get(sut.DefaultFileOperations);
			var verifiable = defaultFolderOperation.Setup(m => m.RenameFile(It.IsAny<CancellationToken>(), newFile.File, oldFile.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), newFile.File)).ReturnsTask(false);
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), oldFile.File)).ReturnsTask(false);
			//act
			await sut.RenameFile(CancellationToken.None, newFile.File, oldFile.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileAutoData]
		public async Task Rename_WhenOldFileDoesNotRequireTranscoding_AndNewFileRequireTranscoding_ShouldCallTranscodingFolderOperationsSynchronize(
		[Frozen]Mock<IRequireTranscoding> requireTranscoding,
		ChooseFolderOperation sut,
		SourceFilePath newFile,
		SourceFilePath oldFile)
		{
			//arrange			
			var transcodingFolderOperation = Mock.Get(sut.TranscodingFolderOperation);
			var verifiable = transcodingFolderOperation.Setup(m => m.SynchronizeFile(It.IsAny<CancellationToken>(), newFile.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), newFile.File)).ReturnsTask(true);
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), oldFile.File)).ReturnsTask(false);
			//act
			await sut.RenameFile(CancellationToken.None, newFile.File, oldFile.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileAutoData]
		public async Task Rename_WhenOldFileDoesNotRequireTranscoding_AndNewFileRequireTranscoding_ShouldDeleteOldFile(
		[Frozen]Mock<IRequireTranscoding> requireTranscoding,
		ChooseFolderOperation sut,
		SourceFilePath newFile,
		SourceFilePath oldFile)
		{
			//arrange			
			var defaultFolderOperation = Mock.Get(sut.DefaultFileOperations);
			var verifiable = defaultFolderOperation.Setup(m => m.DeleteFile(It.IsAny<CancellationToken>(), oldFile.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), newFile.File)).ReturnsTask(true);
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), oldFile.File)).ReturnsTask(false);
			//act
			await sut.RenameFile(CancellationToken.None, newFile.File, oldFile.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileAutoData]
		public async Task Rename_WhenOldFileRequireTranscoding_AndNewFileDoesNotRequireTranscoding_ShouldCallDefaultFolderOperationsSynchronize(
		[Frozen]Mock<IRequireTranscoding> requireTranscoding,
		ChooseFolderOperation sut,
		SourceFilePath newFile,
		SourceFilePath oldFile)
		{
			//arrange			
			var defaultFolderOperation = Mock.Get(sut.DefaultFileOperations);
			var verifiable = defaultFolderOperation.Setup(m => m.SynchronizeFile(It.IsAny<CancellationToken>(), newFile.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), newFile.File)).ReturnsTask(false);
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), oldFile.File)).ReturnsTask(true);
			//act
			await sut.RenameFile(CancellationToken.None, newFile.File, oldFile.File);
			//assert
			verifiable.Verify();
		}

		[Theory, FileAutoData]
		public async Task Rename_WhenOldFileRequireTranscoding_AndNewFileDoesNotRequireTranscoding_ShouldDeleteOldFile(
		[Frozen]Mock<IRequireTranscoding> requireTranscoding,
		ChooseFolderOperation sut,
		SourceFilePath newFile,
		SourceFilePath oldFile)
		{
			//arrange			
			var transcodingFolderOperation = Mock.Get(sut.TranscodingFolderOperation);
			var verifiable = transcodingFolderOperation.Setup(m => m.DeleteFile(It.IsAny<CancellationToken>(), oldFile.File)).ReturnsDefaultTaskVerifiable();
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), newFile.File)).ReturnsTask(false);
			requireTranscoding.Setup((r) => r.ForFile(It.IsAny<CancellationToken>(), oldFile.File)).ReturnsTask(true);
			//act
			await sut.RenameFile(CancellationToken.None, newFile.File, oldFile.File);
			//assert
			verifiable.Verify();
		}
	}
}
