﻿using System;
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
using System.Reactive.Subjects;
using Moq;
using static Microsoft.Reactive.Testing.ReactiveTest;
using System.Reactive;
using System.Reactive.Linq;

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


        #region ISynchronizationController
        [Theory, DomainRxAutoData]
        public void ObserveSynchronizationIsEnabled_ShouldReturnCorrectValue(
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
        public void ObserveSynchronizationIsEnabled_WhenCallingEnable_ShouldReturnCorrectValue(
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
        public void ObserveSynchronizationIsEnabled_AfterCallingEnable_ShouldReturnCorrectValue(
            [Frozen]TestScheduler scheduler,
            SynchronizationController sut)
        {
            //arrange			
            sut.Enable();
            scheduler.AdvanceTo(500);
            //act
            var actual = scheduler.Start(() => sut.ObserveSynchronizationIsEnabled());
            //assert
            actual.Values().Should().BeEquivalentTo(new[] { true });
        }

        [Theory, DomainRxAutoData]
        public void ObserveSynchronizationIsEnabled_WhenSynchronizationIsEnabled_AndDisableIsCalled_ShouldReturnCorrectValue(
            [Frozen]TestScheduler scheduler,
            SynchronizationController sut)
        {
            //arrange			
            scheduler.Schedule(TimeSpan.FromTicks(300), () => sut.Enable());
            scheduler.Schedule(TimeSpan.FromTicks(301), () => sut.Disable());
            //act
            var actual = scheduler.Start(() => sut.ObserveSynchronizationIsEnabled());
            //assert
            actual.Values().Should().BeEquivalentTo(new[] { false, true, false });
        }

        [Theory, DomainRxAutoData]
        public void ObserveSynchronizationIsEnabled_ShouldNotBeSubject(
            SynchronizationController sut)
        {
            //act and assert
            sut.ObserveSynchronizationIsEnabled().As<ISubject<bool>>().Should().BeNull();
        }

        [Theory, DomainRxAutoData]
        public void Enable_ShouldCallStart(
            [Frozen]Mock<IStartSynchronizing> startSynchronizing,
            SynchronizationController sut)
        {
            //act
            sut.Enable();
            //assert
            startSynchronizing.Verify(s => s.Start());
        }

        [Theory, DomainRxAutoData]
        public void Disable_WhenCallingStart_ShouldCallDispose(
           [Frozen]TestScheduler scheduler,
           [Frozen]Mock<IStartSynchronizing> startSynchronizing,
           SynchronizationController sut,
           Mock<IDisposable> disposable)
        {
            startSynchronizing.Setup(s => s.Start()).Returns(disposable.Object);
            scheduler.Schedule(300.Ticks(), () => sut.Enable());
            //act
            scheduler.Schedule(301.Ticks(), () => sut.Disable());
            scheduler.Start();
            //assert
            disposable.Verify(d => d.Dispose());
        }
        #endregion

        
    }
}
