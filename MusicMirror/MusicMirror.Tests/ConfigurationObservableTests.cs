

using System;
using System.Linq;
using FluentAssertions;
using Hanno.Services;
using Microsoft.Reactive.Testing;
using Moq;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using MusicMirror.Tests.Autofixture;
using static Hanno.Testing.Autofixture.TestableObserverExtensions;

namespace MusicMirror.Tests
{
	public class ConfigurationObservableTests : ReactiveTest
	{
		[Theory, AutoMoqData]
		public void Sut_IsIObservable(
		  ConfigurationObservable sut)
		{
			sut.Should().BeAssignableTo<IObservable<Configuration>>();
		}

		[Theory, AutoMoqData]
		public void Sut_VerifyGuardClauses(
		  GuardClauseAssertion assertion)
		{
			assertion.VerifyType<ConfigurationObservable>();
		}

		[Theory, AutoMoqData]
		public void Sut_ShouldReturnCorrectValue(
			[Frozen]Mock<ISettingsService> settingsService,
			ConfigurationObservable sut,
			TestScheduler scheduler,
			Configuration expected)
		{
			//arrange
			settingsService.Setup(s => s.ObserveValue(ConfigurationObservable.SourcePathKey, It.IsAny<Func<string>>()))
						   .Returns(scheduler.CreateColdObservable(OnNext(0, expected.SourcePath.FullName)));
			settingsService.Setup(s => s.ObserveValue(ConfigurationObservable.TargetPathKey, It.IsAny<Func<string>>()))
						   .Returns(scheduler.CreateColdObservable(OnNext(0, expected.TargetPath.FullName)));
			//act
			var result = scheduler.Start(() => sut);
			//assert
			result.Values().First().Should().Be(expected);
		}

		[Theory, AutoMoqData]
		public void Sut_WhenSettingsSendSameValueTwice_ShouldReturnCorrectValue(
			[Frozen]Mock<ISettingsService> settingsService,
			ConfigurationObservable sut,
			TestScheduler scheduler,
			Configuration expected)
		{
			//arrange
			settingsService.Setup(s => s.ObserveValue(ConfigurationObservable.SourcePathKey, It.IsAny<Func<string>>()))
						   .Returns(scheduler.CreateColdObservable(
						   OnNext(0, expected.SourcePath.FullName),
						   OnNext(1, expected.SourcePath.FullName)
						   ));
			settingsService.Setup(s => s.ObserveValue(ConfigurationObservable.TargetPathKey, It.IsAny<Func<string>>()))
						   .Returns(scheduler.CreateColdObservable(
						   OnNext(0, expected.TargetPath.FullName),
						   OnNext(1, expected.TargetPath.FullName)
						   ));
			//act
			var result = scheduler.Start(() => sut);
			//assert
			result.Values().Count().Should().Be(1);
		}

		[Theory, AutoMoqData]
		public void Sut_WhenUsingDefaultValue_ShouldReturnCorrectValue(
			[Frozen]Mock<ISettingsService> settingsService,
			ConfigurationObservable sut,
			TestScheduler scheduler
			)
		{
			//arrange
			settingsService.Setup(s => s.ObserveValue(ConfigurationObservable.SourcePathKey, It.IsAny<Func<string>>()))
						   .Returns<string, Func<string>>((key, defaultValue) => scheduler.CreateColdObservable(
							OnNext(0, defaultValue())
							));
			settingsService.Setup(s => s.ObserveValue(ConfigurationObservable.TargetPathKey, It.IsAny<Func<string>>()))
						   .Returns<string, Func<string>>((key, defaultValue) => scheduler.CreateColdObservable(
							OnNext(0, defaultValue())
							));
			var expected = new Configuration(null, null, NonTranscodingFilesBehavior.Copy);
			//act
			var result = scheduler.Start(() => sut);
			//assert
			result.Values().First().ShouldBeEquivalentTo(expected);
		}
	}
}