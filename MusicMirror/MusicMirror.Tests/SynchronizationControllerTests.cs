using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Xunit;
using Ploeh.AutoFixture.Xunit2;
using Microsoft.Reactive.Testing;
using Hanno.Testing.Autofixture;
using System.Reactive.Concurrency;
using MusicMirror.Tests.Customizations;
using System.Reactive.Disposables;

namespace MusicMirror.Tests
{
	public sealed class SynchronizationControllerTests
	{

		[Theory, DomainRxAutoData]
		public void Sut_VerifyGuardClauses(GuardClauseAssertion assertion)
		{
			assertion.VerifyType<SynchronizationController>();
		}

		[Theory, DomainRxAutoData]
		public void Sut_ConstructorInitialization(ConstructorInitializedMemberAssertion assertion)
		{
			assertion.VerifyConstructors<SynchronizationController>();
		}


		[Theory, DomainRxAutoData]
		public void IsEnabled_ShouldReturnCorrectValue(
			[Frozen]TestScheduler scheduler,
			SynchronizationController sut)
		{
			//arrange
			//act
			var actual = scheduler.Start(() => sut.ObserveSynchronizationIsEnabled());
			//assert
			actual.Values().Should().BeEquivalentTo(new[] { false });
		}

		[Theory, DomainRxAutoData]
		public void IsEnabled_WhenCallingEnable_ShouldReturnCorrectValue(
			[Frozen]TestScheduler scheduler,
			SynchronizationController sut)
		{
			//arrange
			scheduler.Schedule(TimeSpan.FromTicks(300), () => sut.Enable());
			//act
			var actual = scheduler.Start(() => sut.ObserveSynchronizationIsEnabled());
			//assert
			actual.Values().Should().BeEquivalentTo(new[] { false, true });
		}

		[Theory, DomainRxAutoData]
		public void IsEnabled_AfterCallingEnable_ShouldReturnCorrectValue(
			[Frozen]TestScheduler scheduler,
			SynchronizationController sut)
		{
			//arrange
			//scheduler.Start();
			sut.Enable();
			scheduler.AdvanceTo(500);
			//act
			var actual = scheduler.Start(() => sut.ObserveSynchronizationIsEnabled());
			//assert
			actual.Values().Should().BeEquivalentTo(new[] { true });
		}

		[Theory, DomainRxAutoData]
		public void IsEnabled_AfterCallingEnableAndDispose_ShouldReturnCorrectValue(
			[Frozen]TestScheduler scheduler,
			SynchronizationController sut)
		{
			//arrange
			IDisposable disposable = Disposable.Empty;
			scheduler.Schedule(TimeSpan.FromTicks(300), () => disposable = sut.Enable());
			scheduler.Schedule(TimeSpan.FromTicks(301), () => disposable.Dispose());
			//act
			var actual = scheduler.Start(() => sut.ObserveSynchronizationIsEnabled());
			//assert
			actual.Values().Should().BeEquivalentTo(new[] { false, true, false });
		}
	}
}
