using FluentAssertions;
using Hanno.Testing.Autofixture;
using Microsoft.Reactive.Testing;
using Moq;
using MusicMirror.Tests.Autofixture;
using MusicMirror.Tests.Customizations;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.Reactive.Testing.ReactiveTest;

namespace MusicMirror.Tests
{
    public class SynchronizeFilesWhenFileChangedTests
    {
        [Theory, FileAutoData]
        public void Sut_ShouldBeIStartSynchronizing(SynchronizeFilesWhenFileChanged sut)
        {
            sut.Should().BeAssignableTo<IStartSynchronizing>();
        }

        [Theory, FileAutoData]
        public void Sut_ShouldBeITranscodingNotifications(SynchronizeFilesWhenFileChanged sut)
        {
            sut.Should().BeAssignableTo<ITranscodingNotifications>();
        }

        [Theory, FileAutoData]
        public void Sut_VerifyGuardClauses(GuardClauseAssertion assertion)
        {
            assertion.VerifyType<SynchronizeFilesWhenFileChanged>();
        }

        [Theory, FileAutoData]
        public void Sut_VerifyInitializedMembers(ConstructorInitializedMemberAssertion assertion)
        {
            assertion.VerifyType<SynchronizeFilesWhenFileChanged>();
        }

        [Theory, FileAutoData]
        public void Start_ShouldReturnDisposable(SynchronizeFilesWhenFileChanged sut)
        {
            sut.Start().Should().NotBeNull();
        }

        [Theory, FileAutoData]
        public void Start_WhenFileNotificationIsYield_ShouldCallSynchronize(
            [Frozen] TestScheduler scheduler,
            [Frozen(As = typeof (IObservable<MusicMirrorConfiguration>))] TestObservableWrapper<MusicMirrorConfiguration> configurationObservable,
            [Frozen] Mock<IFileObserverFactory> fileObserverFactory,
            [Frozen] Mock<IFileSynchronizerVisitorFactory> fileSynchronizerFactory,
            SynchronizeFilesWhenFileChanged sut,
            MusicMirrorConfiguration configuration,
            IFileNotification[][] fileNotification,
            Mock<IFileSynchronizerVisitor> fileSynchronizerVisitor)
        {
            //arrange
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            const long NotificationsStart = Subscribed + 2;
            var fileObserver = scheduler.CreateHotObservable(
                fileNotification.Select((f, i) => OnNext(NotificationsStart + i, f)).ToArray());
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            fileSynchronizerFactory.Setup(f => f.CreateVisitor(configuration)).Returns(fileSynchronizerVisitor.Object);
            //act            
            sut.Start();
            scheduler.Start();
            //assert                        
            foreach (var m in fileNotification.SelectMany(f => f).Select(f => Mock.Get(f)))
            {
                m.Verify(ff => ff.Accept(It.IsAny<CancellationToken>(), fileSynchronizerVisitor.Object));
            }
        }

        #region ObserveTranscodingResult
        [Theory, FileAutoData]
        public void ObserveTranscodingResult_WhenFileNotificationIsYield_ShouldReturnCorrectValue(
            [Frozen]TestScheduler scheduler,
            [Frozen(As = typeof(IObservable<MusicMirrorConfiguration>))]TestObservableWrapper<MusicMirrorConfiguration> configurationObservable,
            [Frozen]Mock<IFileObserverFactory> fileObserverFactory,
            [Frozen]Mock<IFileSynchronizerVisitorFactory> fileSynchronizerFactory,
            SynchronizeFilesWhenFileChanged sut,
            MusicMirrorConfiguration configuration,
            IFileNotification[][] fileNotification,
            Mock<IFileSynchronizerVisitor> fileSynchronizerVisitor)
        {
            //arrange
            sut.SynchronizationScheduler = ImmediateScheduler.Instance;
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            const long NotificationsStart = Subscribed + 2;
            var fileObserver = scheduler.CreateHotObservable(
                fileNotification.Select((f, i) => OnNext(NotificationsStart + i, f)).ToArray());
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            fileSynchronizerFactory.Setup(f => f.CreateVisitor(configuration)).Returns(fileSynchronizerVisitor.Object);
            scheduler.Schedule(200.Ticks(), () => sut.Start());
            //act         
            var actual = scheduler.Start(() => sut.ObserveTranscodingResult());
            //assert            
            var expected = fileNotification.SelectMany(
                (notifications, i) => notifications.Select(
                    f => new FileTranscodingResultNotification.SuccessTranscodingResultNotification(f))
                    ).ToArray();
            actual.Values().ShouldAllBeEquivalentTo(expected, o => o.RespectingRuntimeTypes());
        }

        [Theory, FileAutoData]
        public void ObserveTranscodingResult_WhenFileNotificationIsYieldAndHasErrors_ShouldReturnCorrectValue(
            [Frozen]TestScheduler scheduler,
            [Frozen(As = typeof(IObservable<MusicMirrorConfiguration>))]TestObservableWrapper<MusicMirrorConfiguration> configurationObservable,
            [Frozen]Mock<IFileObserverFactory> fileObserverFactory,
            [Frozen]Mock<IFileSynchronizerVisitorFactory> fileSynchronizerFactory,
            SynchronizeFilesWhenFileChanged sut,
            MusicMirrorConfiguration configuration,
            IFileNotification[][] fileNotification,
            Mock<IFileSynchronizerVisitor> fileSynchronizerVisitor,
            Exception expectedException)
        {
            //arrange
            sut.SynchronizationScheduler = ImmediateScheduler.Instance;
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            const long NotificationsStart = Subscribed + 2;
            var fileObserver = scheduler.CreateHotObservable(
                fileNotification.Select((f, i) => OnNext(NotificationsStart + i, f)).ToArray());
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            fileSynchronizerFactory.Setup(f => f.CreateVisitor(configuration)).Returns(fileSynchronizerVisitor.Object);
            foreach (var f in fileNotification)
            {
                foreach (var ff in f)
                {
                    Mock.Get(ff).Setup(fff => fff.Accept(It.IsAny<CancellationToken>(), fileSynchronizerVisitor.Object)).Throws(expectedException);
                }
            }
            scheduler.Schedule(200.Ticks(), () => sut.Start());
            //act         
            var actual = scheduler.Start(() => sut.ObserveTranscodingResult());
            //assert            
            var expected = fileNotification.SelectMany(
                (notifications, i) => notifications.Select(
                    f => new FileTranscodingResultNotification.FailureTranscodingResultNotification(f, expectedException))
                    ).ToArray();
            actual.Values().ShouldAllBeEquivalentTo(expected, o => o.RespectingRuntimeTypes());
        }

        [Theory, FileAutoData]
        public void ObserveTranscodingResult_ShouldNoBeSubject(SynchronizeFilesWhenFileChanged sut)
        {
            (sut.ObserveTranscodingResult() as ISubject<IFileTranscodingResultNotification>).Should().BeNull("IObservable<T> is also a ISubject<T>, but it should not");
        }
        #endregion

        #region ObserveFileNotifications
        [Theory, FileAutoData]
        public void ObserveFileNotifications_Start_WhenFileNotificationIsYield_ShouldCallSynchronize(
        [Frozen]TestScheduler scheduler,
        [Frozen(As = typeof(IObservable<MusicMirrorConfiguration>))]TestObservableWrapper<MusicMirrorConfiguration> configurationObservable,
        [Frozen]Mock<IFileObserverFactory> fileObserverFactory,
        SynchronizeFilesWhenFileChanged sut,
        MusicMirrorConfiguration configuration,
        IFileNotification[][] fileNotification)
        {
            //arrange
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            var expectedNotifications = fileNotification.Select((f, i) => OnNext(Subscribed + i + 2, f)).ToArray();
            var fileObserver = scheduler.CreateHotObservable(expectedNotifications);
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            scheduler.Schedule(200.Ticks(), () => sut.Start());
            //act            
            var actual = scheduler.Start(() => sut.ObserveNotifications());
            //assert            
            var expected = expectedNotifications.Select(n => n.Value.Value);
            actual.Values().ShouldAllBeEquivalentTo(expected);
        }

        [Theory, FileAutoData]
        public void ObserveNotifications_ShouldNoBeSubject(SynchronizeFilesWhenFileChanged sut)
        {
            (sut.ObserveNotifications() as ISubject<IFileNotification[]>).Should().BeNull("IObservable<T> is also a ISubject<T>, but it should not");
        }
        #endregion

        #region ObserveIsTranscodingRunning
        [Theory, FileAutoData]
        public void ObserveIsTranscodingRunning_WhenNoFileIsTranscoding_ShouldReturnCorrectValue(
            [Frozen]TestScheduler scheduler,
            SynchronizeFilesWhenFileChanged sut
            )
        {
            //act
            var actual = scheduler.Start(() => sut.ObserveIsTranscodingRunning());
            //assert
            actual.Messages.ShouldAllBeEquivalentTo(OnNext(Subscribed, false).Yield());
        }

        [Theory, FileAutoData]
        public void ObserveIsTranscodingRunning_WhenFilesAreTranscoding_ShouldReturnCorrectValue(
            [Frozen]TestScheduler scheduler,
            [Frozen(As = typeof(IObservable<MusicMirrorConfiguration>))]TestObservableWrapper<MusicMirrorConfiguration> configurationObservable,
            [Frozen]Mock<IFileObserverFactory> fileObserverFactory,
            SynchronizeFilesWhenFileChanged sut,
            MusicMirrorConfiguration configuration,
            IFileNotification[] fileNotification
            )
        {
            //arrange
            sut.SynchronizationScheduler = ImmediateScheduler.Instance;
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            const long NotificationsStart = Subscribed + 2;
            var fileObserver = scheduler.CreateHotObservable(OnNext(NotificationsStart, fileNotification));
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            SetupTranscodingWorkWithIncrementalDuration(scheduler, fileNotification);
            scheduler.Schedule(200.Ticks(), () => sut.Start());
            //act            
            var actual = scheduler.Start(() => sut.ObserveIsTranscodingRunning());
            //assert
            var expected = new[] { false, true, false };
            actual.Values().ShouldAllBeEquivalentTo(expected);
        }

        [Theory, FileAutoData]
        public void ObserveIsTranscodingRunning_WhenFilesAreTranscodingAndSubscribeOccursLaterOn_ShouldReturnCorrectValue(
           [Frozen]TestScheduler scheduler,
           [Frozen(As = typeof(IObservable<MusicMirrorConfiguration>))]TestObservableWrapper<MusicMirrorConfiguration> configurationObservable,
           [Frozen]Mock<IFileObserverFactory> fileObserverFactory,
           SynchronizeFilesWhenFileChanged sut,
           MusicMirrorConfiguration configuration,
           IFileNotification[] fileNotification
           )
        {
            sut.SynchronizationScheduler = ImmediateScheduler.Instance;
            const long NotificationsStart = Subscribed + 1;
            const long Subscription = Subscribed + 3;
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(NotificationsStart, configuration)));
            var fileObserver = scheduler.CreateHotObservable(OnNext(NotificationsStart, fileNotification));
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            SetupTranscodingWorkWithIncrementalDuration(scheduler, fileNotification);
            scheduler.Schedule(200.Ticks(), () => sut.Start());

            //act            
            var actual = scheduler.Start(() => sut.ObserveIsTranscodingRunning(), Created, Subscription, Disposed);
            //assert
            var expected = new[] { true, false };                   
            actual.Values().ShouldAllBeEquivalentTo(expected);
        }


        [Theory, FileAutoData]
        public void ObserveIsTranscodingRunning_WhenFilesAreTranscodingAndStartIsDisposed_ShouldReturnCorrectValue(
           [Frozen]TestScheduler scheduler,
           [Frozen(As = typeof(IObservable<MusicMirrorConfiguration>))]TestObservableWrapper<MusicMirrorConfiguration> configurationObservable,                
           [Frozen]Mock<IFileObserverFactory> fileObserverFactory,
           SynchronizeFilesWhenFileChanged sut,
           MusicMirrorConfiguration configuration,
           IFileNotification[] fileNotification
           )
        {
            sut.SynchronizationScheduler = ImmediateScheduler.Instance;
            const long NotificationsStart = Subscribed + 2;
            const long DisposeStart = Subscribed + 6;
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(NotificationsStart, configuration)));            
            var fileObserver = scheduler.CreateHotObservable(OnNext(NotificationsStart, fileNotification));
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            SetupTranscodingWorkWithInfiniteDuration(scheduler, fileNotification);           
            scheduler.Schedule(201.Ticks(), () =>
            {
                var disposable = sut.Start();
                scheduler.ScheduleAbsolute(DisposeStart, () => disposable.Dispose());
            });
            //act            
            var actual = scheduler.Start(sut.ObserveIsTranscodingRunning, Created, Subscribed, Subscribed + 20);
            //assert
            var expected = new[] { false, true, false };                        
            actual.Values().ShouldAllBeEquivalentTo(expected);
        }

        private static void SetupTranscodingWorkWithIncrementalDuration(TestScheduler scheduler, IFileNotification[] fileNotification)
        {
            var i = 0;
            foreach (var f in fileNotification)
            {
                i++;
                var j = i;
                Mock.Get(f)
                    .Setup(ff => ff.Accept(It.IsAny<CancellationToken>(), It.IsAny<IFileSynchronizerVisitor>()))
                    .Returns(() =>
                    {
                        var completionSource = new TaskCompletionSource<bool>();
                        scheduler.ScheduleAbsolute(scheduler.Now.AddTicks(j).Ticks, () => completionSource.SetResult(true));
                        return completionSource.Task;
                    });
            }
        }

        private static void SetupTranscodingWorkWithInfiniteDuration(TestScheduler scheduler, IFileNotification[] fileNotification)
        {
            foreach (var f in fileNotification)
            {
                Mock.Get(f)
                    .Setup(ff => ff.Accept(It.IsAny<CancellationToken>(), It.IsAny<IFileSynchronizerVisitor>()))
                    .Returns(new TaskCompletionSource<bool>().Task);
            }
        }
        #endregion
    }
}
