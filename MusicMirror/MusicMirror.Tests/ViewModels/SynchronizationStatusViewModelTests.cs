using FluentAssertions;
using Hanno.Testing.Autofixture;
using Microsoft.Reactive.Testing;
using Moq;
using MusicMirror.Tests.Customizations;
using MusicMirror.ViewModels;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.Reactive.Testing.ReactiveTest;

namespace MusicMirror.Tests
{
    public class SynchronizationStatusViewModelTests
    {
        [Theory, ViewModelAutoData]
        public void IsTranscodingRunning_WhenSynchronizationIsDisabled_ShouldReturnCorrectValue(
            [Frozen]TestSchedulers schedulers,
            [Frozen]Mock<ITranscodingNotifications> notifications,
            SynchronizationStatusViewModel sut)
        {
            //act
            var actual = schedulers.Start(() => sut.IsTranscodingRunning);
            //assert
            var expected = new[]
            {
                OnNext(200, false)
            };
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }

        [Theory, ViewModelAutoData]
        public void IsTranscodingRunning_WhenTranscodingIsRunning_ShouldReturnCorrectValue(
           [Frozen]TestSchedulers schedulers,
           [Frozen]Mock<ITranscodingNotifications> notifications,
           Fixture fixture)
        {
            //arrange
            var notificationsObservable = schedulers.CreateHotObservable(
                OnNext(202, false),
                OnNext(203, false),
                OnNext(204, true),
                OnNext(205, false)
                );
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);
            var sut = fixture.Create<SynchronizationStatusViewModel>();
            //act
            var actual = schedulers.Start(() => sut.IsTranscodingRunning);
            //assert
            var expected = new[]
            {
                OnNext(200, false),
                OnNext(204, true),
                OnNext(205, false)
            };
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }


        [Theory, ViewModelAutoData]
        public void SynchronizedFileCount_ShouldReturnCorrectValue(
              [Frozen]TestSchedulers schedulers,
              [Frozen]Mock<INotificationViewModelProducer> notificationsViewModelProducer,
              SynchronizedFilesCountViewModel[] expected,
              Fixture fixture)
        {
            //arrange
            var observable = schedulers.CreateHotObservable<SynchronizedFilesCountViewModel>(
                expected.Select((f, i) => OnNext(Subscribed + 5, f)).ToArray());
            notificationsViewModelProducer.Setup(n => n.ObserveSynchronizedFileCount()).Returns(observable);
            var sut = fixture.Create<SynchronizationStatusViewModel>();
            //act
            var actual = schedulers.Start(() => sut.SynchronizedFileCount);
            //assert
            actual.Values().ShouldAllBeEquivalentTo(expected);
        }

    }
}
