using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hanno.IO;
using Moq;
using MusicMirror.Synchronization;
using MusicMirror.Tests.Autofixture;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using static Hanno.Testing.Autofixture.MockExtensions;

namespace MusicMirror.Tests
{
	public class SynchronizeFileServiceTests
	{
		[Theory, AutoMoqData]
		public void Sut_ShouldBeSynchronizeFileService(SynchronizeFileService sut)
		{
			sut.Should().BeAssignableTo<IMirroredFolderOperations>();
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyGuardClauses(GuardClauseAssertion assertion)
		{
			assertion.VerifyType<SynchronizeFileService>();
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyConstructorInitializedMember(ConstructorInitializedMemberAssertion assertion)
		{
			assertion.VerifyType<SynchronizeFileService>();
		}

		[Theory, FileAutoData]
		public async Task RenameFile_ShouldCallRenameFile(
			[Frozen]Mock<ISynchronizedFilesRepository> syncFilesRepository,
			[Frozen]Mock<IAsyncFileOperations> fileOperations,
			  SynchronizeFileService sut,
			  SourceFilePath newFilePath,
			  SourceFilePath oldFilePath,
			  TargetFilePath oldMirroredFilePath,
			  TargetFilePath newMirroredFilePath
			  )
		{
			//arrange											
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), oldFilePath.File))
							   .ReturnsTask(oldMirroredFilePath.File);
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), newFilePath.File))
							   .ReturnsTask(newMirroredFilePath.File);
			//act
			await sut.RenameFile(CancellationToken.None, newFilePath.File, oldFilePath.File);
			//assert
			fileOperations.Verify(f => f.Move(oldMirroredFilePath.ToString(), newMirroredFilePath.ToString()));
		}

		[Theory, FileAutoData]
		public async Task RenameFile_ShouldCallAddSynchronization(
			[Frozen]Mock<ISynchronizedFilesRepository> syncFilesRepository,
			[Frozen]Mock<IAsyncFileOperations> fileOperations,
			  SynchronizeFileService sut,
			  SourceFilePath newFilePath,
			  SourceFilePath oldFilePath,
			  TargetFilePath oldMirroredFilePath
			  )
		{
			//arrange			
			var expectedTargetFilePath = Path.Combine(
				sut.Configuration.TargetPath.FullName,
				newFilePath.RelativePath,
				oldMirroredFilePath.File.Name);
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), oldFilePath.File))
							   .ReturnsTask(oldMirroredFilePath.File);
			//act
			await sut.RenameFile(CancellationToken.None, newFilePath.File, oldFilePath.File);
			//assert
			syncFilesRepository.Verify(f => f.AddSynchronization(
				It.IsAny<CancellationToken>(),
				newFilePath.File,
				It.IsAny<DateTimeOffset>()));
		}

		[Theory, FileAutoData]
		public async Task RenameFile_ShouldCallDeleteSynchronization(
			[Frozen]Mock<ISynchronizedFilesRepository> syncFilesRepository,
			[Frozen]Mock<IAsyncFileOperations> fileOperations,
			  SynchronizeFileService sut,
			  SourceFilePath newFilePath,
			  SourceFilePath oldFilePath,
			  TargetFilePath oldMirroredFilePath
			  )
		{
			//arrange			
			var expectedTargetFilePath = Path.Combine(
				sut.Configuration.TargetPath.FullName,
				newFilePath.RelativePath,
				oldMirroredFilePath.File.Name);
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), oldFilePath.File))
							   .ReturnsTask(oldMirroredFilePath.File);
			//act
			await sut.RenameFile(CancellationToken.None, newFilePath.File, oldFilePath.File);
			//assert
			syncFilesRepository.Verify(f => f.DeleteSynchronization(
				It.IsAny<CancellationToken>(),
				oldFilePath.File));
		}

		[Theory, FileAutoData]
		public async Task DeleteFile_ShouldCallDeleteFile(
			[Frozen]Mock<ISynchronizedFilesRepository> syncFilesRepository,
			[Frozen]Mock<IAsyncFileOperations> fileOperations,
			  SynchronizeFileService sut,
			  SourceFilePath sourceFilePath,
			  TargetFilePath mirroredFilePath
			  )
		{
			//arrange			
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), sourceFilePath.File))
							   .ReturnsTask(mirroredFilePath.File);
			//act
			await sut.DeleteFile(CancellationToken.None, sourceFilePath.File);
			//assert
			fileOperations.Verify(f => f.Delete(mirroredFilePath.ToString()));
		}

		[Theory, FileAutoData]
		public async Task DeleteFile_ShouldCallDeleteSynchronization(
			[Frozen]Mock<ISynchronizedFilesRepository> syncFilesRepository,
			[Frozen]Mock<IAsyncFileOperations> fileOperations,
			  SynchronizeFileService sut,
			  SourceFilePath sourceFilePath,
			  TargetFilePath mirroredFilePath
			  )
		{
			//arrange			
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), sourceFilePath.File))
							   .ReturnsTask(mirroredFilePath.File);
			//act
			await sut.DeleteFile(CancellationToken.None, sourceFilePath.File);
			//assert
			syncFilesRepository.Verify(f => f.DeleteSynchronization(It.IsAny<CancellationToken>(), sourceFilePath.File));
		}

		[Theory, 
			FileInlineAutoData(true),
			FileInlineAutoData(false)]
		public async Task HasMirroredFileForPath_ShouldReturnCorrectValue(
			bool expected,
			[Frozen]Mock<ISynchronizedFilesRepository> syncFilesRepository,
			[Frozen]Mock<IAsyncFileOperations> fileOperations,
			  SynchronizeFileService sut,
			  SourceFilePath sourceFilePath,
			  TargetFilePath mirroredFilePath
			  )
		{
			//arrange			
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), sourceFilePath.File))
							   .ReturnsTask(mirroredFilePath.File);
			var tf = mirroredFilePath.ToString();
			fileOperations.Setup(f => f.Exists(tf)).ReturnsTask(expected);
			//act
			var actual = await sut.HasMirroredFileForPath(CancellationToken.None, sourceFilePath.File);
			//assert
			actual.Should().Be(expected);
		}

		[Theory,
		FileAutoData]
		public async Task SynchronizeFile_ShouldCallSynchronizer(		
		[Frozen]Mock<ISynchronizedFilesRepository> syncFilesRepository,
		[Frozen]Mock<IFileSynchronizer> synchronizer,
		  SynchronizeFileService sut,
		  SourceFilePath sourceFilePath,
		  TargetFilePath mirroredFilePath
		  )
		{
			//arrange			
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), sourceFilePath.File))
							   .ReturnsTask(mirroredFilePath.File);
			//act			
			await sut.SynchronizeFile(CancellationToken.None, sourceFilePath.File);
			//assert
			synchronizer.Verify(s => s.Synchronize(It.IsAny<CancellationToken>(), It.Is((IFileInfo f) => sourceFilePath.File == f.File)));
		}

		[Theory,
		FileAutoData]
		public async Task SynchronizeFile_ShouldCallAddSynchronization(
		[Frozen]Mock<ISynchronizedFilesRepository> syncFilesRepository,
		[Frozen]Mock<IFileSynchronizer> synchronizer,
		  SynchronizeFileService sut,
		  SourceFilePath sourceFilePath,
		  TargetFilePath mirroredFilePath
		  )
		{
			//arrange			
			syncFilesRepository.Setup(s => s.GetMirroredFilePath(It.IsAny<CancellationToken>(), sourceFilePath.File))
							   .ReturnsTask(mirroredFilePath.File);
			//act			
			await sut.SynchronizeFile(CancellationToken.None, sourceFilePath.File);
			//assert
			var c = new FileInfoEqualityComparer();
			syncFilesRepository.Verify(
				s => s.AddSynchronization(
					It.IsAny<CancellationToken>(),
					It.Is<FileInfo>(f => c.Equals(f, sourceFilePath.File)),
					It.IsAny<DateTimeOffset>()
					)
				);
		}
	}
}
