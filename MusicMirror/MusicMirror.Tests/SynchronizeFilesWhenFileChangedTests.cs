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
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
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
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            var fileObserver = scheduler.CreateHotObservable(
                fileNotification.Select((f, i) => OnNext(Subscribed + i + 2, f)).ToArray());
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            fileSynchronizerFactory.Setup(f => f.CreateVisitor(configuration)).Returns(fileSynchronizerVisitor.Object);
            var verifiable = fileNotification.SelectMany(f => f)
                                             .Select(f => Mock.Get(f)
                                                              .Setup(ff => ff.Accept(It.IsAny<CancellationToken>(), fileSynchronizerVisitor.Object))
                                                              .ReturnsDefaultTaskVerifiable())
                                             .ToArray()
                                             .AsITaskVerifies();
            //act            
            sut.Start();
            scheduler.Start();
            //assert            
            verifiable.Verify();
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
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            var fileObserver = scheduler.CreateHotObservable(
                fileNotification.Select((f, i) => OnNext(Subscribed + i + 2, f)).ToArray());
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            fileSynchronizerFactory.Setup(f => f.CreateVisitor(configuration)).Returns(fileSynchronizerVisitor.Object);
            scheduler.Schedule(200.Ticks(), () => sut.Start());
            //act         
            var actual = scheduler.Start(() => sut.ObserveTranscodingResult());
            //assert            
            var expected = fileNotification.SelectMany(
                (notifications, i) => notifications.Select(
                    f => OnNext(Subscribed + i + 2, new FileTranscodingResultNotification.SuccessTranscodingResultNotification(f))
                    )
                    ).ToArray();
            actual.Messages.ShouldAllBeEquivalentTo(expected, o => o.RespectingRuntimeTypes());
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
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            var fileObserver = scheduler.CreateHotObservable(
                fileNotification.Select((f, i) => OnNext(Subscribed + i + 2, f)).ToArray());
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
                    f => OnNext(Subscribed + i + 2, new FileTranscodingResultNotification.FailureTranscodingResultNotification(f, expectedException))
                    )
                    ).ToArray();
            actual.Messages.ShouldAllBeEquivalentTo(expected, o => o.RespectingRuntimeTypes());
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
            actual.Messages.ShouldAllBeEquivalentTo(expectedNotifications);
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
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            var fileObserver = scheduler.CreateHotObservable(OnNext(Subscribed + 2, fileNotification));
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            foreach (var f in fileNotification)
            {
                Mock.Get(f).Setup(ff => ff.Accept(It.IsAny<CancellationToken>(), It.IsAny<IFileSynchronizerVisitor>()))
                           .Returns(() =>
                           {
                               scheduler.Sleep(1);
                               return Task.FromResult(true);
                           });
            }
            scheduler.Schedule(200.Ticks(), () => sut.Start());
            //act            
            var actual = scheduler.Start(() => sut.ObserveIsTranscodingRunning());
            //assert
            var expected = new[] {
                OnNext(Subscribed, false),
                OnNext(Subscribed + 2, true),
                OnNext(Subscribed + 2 + fileNotification.Length, false)
            };
            actual.Messages.ShouldAllBeEquivalentTo(expected);
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
            configurationObservable.SetObservable(scheduler.CreateHotObservable(OnNext(Subscribed + 1, configuration)));
            var fileObserver = scheduler.CreateHotObservable(OnNext(Subscribed + 2, fileNotification));
            fileObserverFactory.Setup(f => f.GetFileObserver(configuration.SourcePath)).Returns(fileObserver);
            var i = 0;
            foreach (var f in fileNotification)
            {                
                var j = ++i + Subscribed + 2;
                Mock.Get(f).Setup(ff => ff.Accept(It.IsAny<CancellationToken>(), It.IsAny<IFileSynchronizerVisitor>()))
                           .Returns(() =>
                           {
                               return Observable.Create<Unit>(o =>
                               {
                                   return scheduler.ScheduleAbsolute(j, () =>
                                    {
                                        o.OnNext(Unit.Default);
                                        o.OnCompleted();
                                    });
                               }).ToTask();
                               //var completionSource = new TaskCompletionSource<bool>();
                               //scheduler.ScheduleAbsolute(j, () => completionSource.SetResult(true));
                               //return completionSource.Task;
                           });
            }
            scheduler.Schedule(200.Ticks(), () => sut.Start());
            scheduler.Schedule(203.Ticks(), () =>
            {
                sut.ObserveIsTranscodingRunning().Subscribe(_ => { });
            });
            //act            
            var actual = scheduler.Start(() => sut.ObserveIsTranscodingRunning(), Created, Subscribed + 3, Disposed);
            //assert
            var expected = new[] {
                OnNext(Subscribed + 3, true),
                OnNext(Subscribed + 2 + fileNotification.Length, false)
            };
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }
        #endregion
    }
}
