using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MusicMirror.Tests.Autofixture;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace MusicMirror.Tests
{
	public class FileAddedNotificationTests : FileNotificationTestsBase<FileAddedNotification>
	{
		protected override Expression<Func<IFileSynchronizerVisitor, Task>> GetExpectedVisitMethod(
			FileAddedNotification expected)
		{
			return v => v.Visit(It.IsAny<CancellationToken>(), expected);
		}
	}

	public class FileDeletedNotificationTests : FileNotificationTestsBase<FileDeletedNotification>
	{
		protected override Expression<Func<IFileSynchronizerVisitor, Task>> GetExpectedVisitMethod(
			FileDeletedNotification expected)
		{
			return v => v.Visit(It.IsAny<CancellationToken>(), expected);
		}
	}

	public class FileRenamedNotificationTests : FileNotificationTestsBase<FileRenamedNotification>
	{
		protected override Expression<Func<IFileSynchronizerVisitor, Task>> GetExpectedVisitMethod(
			FileRenamedNotification expected)
		{
			return v => v.Visit(It.IsAny<CancellationToken>(), expected);
		}
	}

	public class FileModifiedNotificationTests : FileNotificationTestsBase<FileModifiedNotification>
	{
		protected override Expression<Func<IFileSynchronizerVisitor, Task>> GetExpectedVisitMethod(
			FileModifiedNotification expected)
		{
			return v => v.Visit(It.IsAny<CancellationToken>(), expected);
		}
	}

	public class FileInitNotificationTests : FileNotificationTestsBase<FileInitNotification>
	{
		protected override Expression<Func<IFileSynchronizerVisitor, Task>> GetExpectedVisitMethod(
			FileInitNotification expected)
		{
			return v => v.Visit(It.IsAny<CancellationToken>(), expected);
		}
	}

	public abstract class FileNotificationTestsBase<T> where T : IFileNotification
	{
		protected abstract Expression<Func<IFileSynchronizerVisitor, Task>> GetExpectedVisitMethod(T expected);

		[Theory, AutoMoqData]
		public async Task Accept_ShouldCallVisitor(
			Mock<IFileSynchronizerVisitor> visitor,
			T sut)
		{
			//arrange
			var expected = GetExpectedVisitMethod(sut);
			//act 
			await sut.Accept(CancellationToken.None, visitor.Object);
			//assert
			visitor.Verify(expected);
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyGuardClauses(
			GuardClauseAssertion assertion)
		{
			assertion.VerifyType<T>();
		}

		[Theory, AutoMoqData]
		public void Sut_FileInfo_ShouldReturnCorrectValue(
			[Frozen]FileInfo fileInfo,
			T sut)
		{
			sut.FileInfo.Should().Be(fileInfo);
		}
	}
}