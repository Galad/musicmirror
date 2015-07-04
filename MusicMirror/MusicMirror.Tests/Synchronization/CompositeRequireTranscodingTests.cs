using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hanno.Testing.Autofixture;
using Moq;
using MusicMirror.Synchronization;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Xunit;

namespace MusicMirror.Tests.Synchronization
{

	public class CompositeRequireTranscodingTests
	{

		[Theory, FileAutoData]
		public void Sut_ShouldBeRequireTranscoding(
			 CompositeRequireTranscoding sut)
		{
			sut.Should().BeAssignableTo<IRequireTranscoding>();
		}

		[Theory, FileAutoData]
		public void Sut_VerifyGuardClauses(
			 GuardClauseAssertion assertion)
		{
			assertion.VerifyType<CompositeRequireTranscoding>();
		}

		[Theory, FileAutoData]
		public void Sut_VerifyConstructorInitialization(
			 ConstructorInitializedMemberAssertion assertion)
		{
			assertion.VerifyType<CompositeRequireTranscoding>();
		}

		[Theory, FileAutoData]
		public async Task ForFile_WhenRequireTranscodingListIsEmpty_ShouldReturnCorrectValue(
			SourceFilePath file,
			IFixture fixture)
		{
			//arrange
			fixture.Inject(Enumerable.Empty<IRequireTranscoding>());
			var sut = fixture.Create<CompositeRequireTranscoding>();
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeFalse();
		}

		[Theory, FileAutoData]
		public async Task ForFile_WhenRequireTranscodingListHasOneItem_AndForFileReturnFalse_ShouldReturnCorrectValue(
		SourceFilePath file,
		IFixture fixture)
		{
			//arrange
			var requireTranscoding = fixture.CreateMany<IRequireTranscoding>(1).ToArray();
			fixture.Inject<IEnumerable<IRequireTranscoding>>(requireTranscoding);
			var sut = fixture.Create<CompositeRequireTranscoding>();
			Mock.Get(requireTranscoding[0]).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeFalse();
		}

		[Theory, FileAutoData]
		public async Task ForFile_WhenRequireTranscodingListHasOneItem_AndForFileReturnTrue_ShouldReturnCorrectValue(
		SourceFilePath file,
		IFixture fixture)
		{
			//arrange
			var requireTranscoding = fixture.CreateMany<IRequireTranscoding>(1).ToArray();
			fixture.Inject<IEnumerable<IRequireTranscoding>>(requireTranscoding);
			var sut = fixture.Create<CompositeRequireTranscoding>();
			Mock.Get(requireTranscoding[0]).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(true);
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeTrue();
		}

		[Theory, FileAutoData]
		public async Task ForFile_WhenRequireTranscodingListHasTwoItems_AndForFileReturnFalse_ShouldReturnCorrectValue(
			SourceFilePath file,
			IFixture fixture)
		{
			//arrange
			var requireTranscoding = fixture.CreateMany<IRequireTranscoding>(2).ToArray();
			fixture.Inject<IEnumerable<IRequireTranscoding>>(requireTranscoding);
			var sut = fixture.Create<CompositeRequireTranscoding>();
			Mock.Get(requireTranscoding[0]).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			Mock.Get(requireTranscoding[1]).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeFalse();
		}


		[Theory,
		FileInlineAutoData(true, false),
		FileInlineAutoData(false, true)]
		public async Task ForFile_WhenRequireTranscodingListHasTwoItems_AndForFileReturnTrue_ShouldReturnCorrectValue(
			bool firstValue,
			bool secondValue,
			SourceFilePath file,
			IFixture fixture)
		{
			//arrange
			var requireTranscoding = fixture.CreateMany<IRequireTranscoding>(2).ToArray();
			fixture.Inject<IEnumerable<IRequireTranscoding>>(requireTranscoding);
			var sut = fixture.Create<CompositeRequireTranscoding>();
			Mock.Get(requireTranscoding[0]).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(firstValue);
			Mock.Get(requireTranscoding[1]).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(secondValue);
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeTrue();
		}

		[Theory, FileAutoData]
		public async Task ForFile_WhenRequireTranscodingListHasManyItems_AndForFileReturnFalse_ShouldReturnCorrectValue(
			SourceFilePath file,
			IFixture fixture,
			int count)
		{
			//arrange
			var requireTranscoding = fixture.CreateMany<IRequireTranscoding>(count + 2).ToArray();
			fixture.Inject<IEnumerable<IRequireTranscoding>>(requireTranscoding);
			var sut = fixture.Create<CompositeRequireTranscoding>();
			foreach (var t in sut.RequireTranscodings)
			{
				Mock.Get(t).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			}
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeFalse();
		}

		[Theory, FileAutoData]
		public async Task ForFile_WhenRequireTranscodingListHasManyItems_AndFirstForFileReturnTrue_ShouldReturnCorrectValue(
			SourceFilePath file,
			IFixture fixture,
			int count)
		{
			//arrange
			var requireTranscoding = fixture.CreateMany<IRequireTranscoding>(count + 2).ToArray();
			fixture.Inject<IEnumerable<IRequireTranscoding>>(requireTranscoding);
			var sut = fixture.Create<CompositeRequireTranscoding>();
			foreach (var t in sut.RequireTranscodings)
			{
				Mock.Get(t).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			}
			Mock.Get(sut.RequireTranscodings.First()).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(true);
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeTrue();
		}

		[Theory, FileAutoData]
		public async Task ForFile_WhenRequireTranscodingListHasManyItems_AndLastForFileReturnTrue_ShouldReturnCorrectValue(
			SourceFilePath file,
			IFixture fixture,
			int count)
		{
			//arrange
			var requireTranscoding = fixture.CreateMany<IRequireTranscoding>(count + 2).ToArray();
			fixture.Inject<IEnumerable<IRequireTranscoding>>(requireTranscoding);
			var sut = fixture.Create<CompositeRequireTranscoding>();
			foreach (var t in sut.RequireTranscodings)
			{
				Mock.Get(t).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			}
			Mock.Get(sut.RequireTranscodings.Last()).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(true);
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeTrue();
		}

		[Theory, FileAutoData]
		public async Task ForFile_WhenRequireTranscodingListHasManyItems_AndMiddleForFileReturnTrue_ShouldReturnCorrectValue(
			SourceFilePath file,
			IFixture fixture,
			int count)
		{
			//arrange
			var requireTranscoding = fixture.CreateMany<IRequireTranscoding>(count + 2).ToArray();
			fixture.Inject<IEnumerable<IRequireTranscoding>>(requireTranscoding);
			var sut = fixture.Create<CompositeRequireTranscoding>();
			foreach (var t in sut.RequireTranscodings)
			{
				Mock.Get(t).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(false);
			}
			Mock.Get(sut.RequireTranscodings.ElementAt(count /2)).Setup(m => m.ForFile(It.IsAny<CancellationToken>(), file.File)).ReturnsTask(true);
			//act
			var actual = await sut.ForFile(CancellationToken.None, file.File);
			//assert
			actual.Should().BeTrue();
		}
	}
}
