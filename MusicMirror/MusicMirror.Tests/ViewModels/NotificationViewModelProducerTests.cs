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
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.Reactive.Testing.ReactiveTest;

namespace MusicMirror.Tests.ViewModels
{
    public class NotificationViewModelProducerTests
    {
        [Theory, FileAutoData]
        public void Sut_ShouldBeNotificationViewModelProducer(
             NotificationViewModelProducer sut)
        {
            sut.Should().BeAssignableTo<INotificationViewModelProducer>();
        }

        [Theory, FileAutoData]
        public void ObserveSynchronizedFileCount_WhenNoFileIsTranscoding_ShouldReturnCorrectValue(
          [Frozen]TestSchedulers schedulers,
          [Frozen]Mock<ITranscodingNotifications> notifications,
          NotificationViewModelProducer sut)
        {
            //arrange
            var notificationsObservable = schedulers.CreateHotObservable<bool>();
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);

            //act
            var actual = schedulers.Start(() => sut.ObserveSynchronizedFileCount());
            //assert
            var expected = new[]
            {
                OnNext(200, SynchronizedFilesCountViewModel.Empty)
            };
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }

        [Theory, FileAutoData]
        public void ObserveSynchronizedFileCount_WhenTranscodingIsNotRunning_AndFileNotificationsArePushed_ShouldReturnCorrectValue(
            [Frozen]TestSchedulers schedulers,
            [Frozen]Mock<ITranscodingNotifications> notifications,
            NotificationViewModelProducer sut,
            IFileNotification[] fileNotifications)
        {
            //arrange
            var fileNotificationsObservable = schedulers.CreateHotObservable(
                OnNext(202, fileNotifications));
            var notificationsObservable = schedulers.CreateHotObservable(
                OnNext(201, false));
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);
            notifications.Setup(n => n.ObserveNotifications()).Returns(fileNotificationsObservable);

            //act
            var actual = schedulers.Start(() => sut.ObserveSynchronizedFileCount());
            //assert
            var expected = new[]
            {
                OnNext(200, SynchronizedFilesCountViewModel.Empty)
            };
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }

        [Theory, FileAutoData]
        public void ObserveSynchronizedFileCount_WhenTranscodingIsRunning_AndFileNotificationsArePushed_ShouldReturnCorrectValue(
            [Frozen]TestSchedulers schedulers,
            [Frozen]Mock<ITranscodingNotifications> notifications,
            IFileNotification[] fileNotifications,
            Fixture fixture)
        {
            //arrange            
            var fileNotificationsObservable = schedulers.CreateHotObservable(
                OnNext(202, fileNotifications));
            var notificationsObservable = schedulers.CreateHotObservable(
                OnNext(201, true));
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);
            notifications.Setup(n => n.ObserveNotifications()).Returns(fileNotificationsObservable);
            var sut = fixture.Create<NotificationViewModelProducer>();
            //act
            var actual = schedulers.Start(() => sut.ObserveSynchronizedFileCount());
            //assert
            var expected = new[]
            {
                OnNext(200, SynchronizedFilesCountViewModel.Empty),
                OnNext(202, new SynchronizedFilesCountViewModel(0, fileNotifications.Length))
            };
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }

        [Theory, FileAutoData]
        public void ObserveSynchronizedFileCount_WhenTranscodingIsRunning_AndFileNotificationsArePushedSeveralTimes_ShouldReturnCorrectValue(
            [Frozen]TestSchedulers schedulers,
            [Frozen]Mock<ITranscodingNotifications> notifications,
            IFileNotification[][] fileNotifications,
            Fixture fixture)
        {
            //arrange            
            var fileNotificationsObservable = schedulers.CreateHotObservable(
                fileNotifications.Select((f, i) => OnNext(Subscribed + i + 101, f)).ToArray());
            var notificationsObservable = schedulers.CreateHotObservable(
                OnNext(Subscribed + 1, true));
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);
            notifications.Setup(n => n.ObserveNotifications()).Returns(fileNotificationsObservable);
            var sut = fixture.Create<NotificationViewModelProducer>();
            //act
            var actual = schedulers.Start(() => sut.ObserveSynchronizedFileCount());
            //assert                                    
            var expected = OnNext(200, SynchronizedFilesCountViewModel.Empty).Yield()
                        .Concat(CreateExpectedFileNotifications(fileNotifications, 101))
                        .ToArray();

            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }

        [Theory, FileAutoData]
        public void ObserveSynchronizedFileCount_WhenTranscodingIsRunningChangesSeveralTimes_AndFileNotificationsArePushedSeveralTimes_ShouldReturnCorrectValue(
            [Frozen]TestSchedulers schedulers,
            [Frozen]Mock<ITranscodingNotifications> notifications,
            IFileNotification[][] fileNotifications,
            Fixture fixture)
        {
            //arrange            
            const int TranscodingStoppedRelativeTime = 50;
            var fileNotificationsObservable = schedulers.CreateHotObservable(
                fileNotifications.Select((f, i) => OnNext(Subscribed + i + 101, f))
                                 .Concat(fileNotifications.Select((f, i) => OnNext(Subscribed + i + 201, f)))
                                 .Concat(fileNotifications.Select((f, i) => OnNext(Subscribed + i + 301, f)))
                                 .ToArray()
                                 );
            var notificationsObservable = schedulers.CreateHotObservable(
                OnNext(Subscribed + 1, false),
                OnNext(Subscribed + 100, true),
                OnNext(Subscribed + 100 + TranscodingStoppedRelativeTime, false),
                OnNext(Subscribed + 200, true),
                OnNext(Subscribed + 200 + TranscodingStoppedRelativeTime, false),
                OnNext(Subscribed + 300, true),
                OnNext(Subscribed + 300 + TranscodingStoppedRelativeTime, false));
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);
            notifications.Setup(n => n.ObserveNotifications()).Returns(fileNotificationsObservable);
            var sut = fixture.Create<NotificationViewModelProducer>();

            //act
            var actual = schedulers.Start(() => sut.ObserveSynchronizedFileCount());
            //assert                                    
            var expected = OnNext(Subscribed, SynchronizedFilesCountViewModel.Empty).Yield()
                        .Concat(CreateExpectedFileNotifications(fileNotifications, 101))
                        .Concat(CreateExpectedFileNotifications(fileNotifications, 201))
                        .Concat(CreateExpectedFileNotifications(fileNotifications, 301))
                        .ToArray();

            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }

        private static IEnumerable<Recorded<System.Reactive.Notification<SynchronizedFilesCountViewModel>>> CreateExpectedFileNotifications(
            IFileNotification[][] fileNotifications,
            int offset)
        {
            return fileNotifications.Aggregate(new List<int>(), (list, files) =>
            {
                list.Add(list.LastOrDefault() + files.Length);
                return list;
            })
                                    .Select((f, i) => OnNext(Subscribed + i + offset, new SynchronizedFilesCountViewModel(0, f)));
        }

        [Theory, FileAutoData]
        public void ObserveSynchronizedFileCount_WhenTranscodingIsRunning_AndTranscodingResultsArePushed_ShouldReturnCorrectValue(
         [Frozen]TestSchedulers schedulers,
         [Frozen]Mock<ITranscodingNotifications> notifications,
         IFileNotification[] fileNotifications,
         Fixture fixture)
        {
            //arrange          
            var notificationsObservable = schedulers.CreateHotObservable(
                OnNext(201, true));
            var transcodingResultsObservable = schedulers.CreateHotObservable(
                fileNotifications.Select((f, i) => OnNext(203 + i, FileTranscodingResultNotification.CreateSuccess(f))).ToArray());
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);
            notifications.Setup(n => n.ObserveTranscodingResult()).Returns(transcodingResultsObservable);
            var sut = fixture.Create<NotificationViewModelProducer>();
            //act
            var actual = schedulers.Start(() => sut.ObserveSynchronizedFileCount());
            //assert
            var expected = new[]
            {
                OnNext(Subscribed, SynchronizedFilesCountViewModel.Empty),
            }
            .Concat(fileNotifications.Select((f, i) => OnNext(203 + i, new SynchronizedFilesCountViewModel(i + 1, 0))).ToArray());
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }

        [Theory, FileAutoData]
        public void ObserveSynchronizedFileCount_WhenTranscodingIsRunningChangesSeveralTimes_AndTranscodingResultsArePushed_ShouldReturnCorrectValue(
         [Frozen]TestSchedulers schedulers,
         [Frozen]Mock<ITranscodingNotifications> notifications,
         IFileNotification[] fileNotifications,
         Fixture fixture)
        {
            //arrange          
            var notificationsObservable = schedulers.CreateHotObservable(
                OnNext(Subscribed + 1, false),
                OnNext(Subscribed + 100, true),
                OnNext(Subscribed + 199, false),
                OnNext(Subscribed + 200, true),
                OnNext(Subscribed + 299, false),
                OnNext(Subscribed + 300, true),
                OnNext(Subscribed + 399, false));
            Func<long, IEnumerable<Recorded<Notification<IFileTranscodingResultNotification>>>> createFileNotifications =
                offset => fileNotifications.Select((f, i) => OnNext(offset + i, FileTranscodingResultNotification.CreateSuccess(f)));
            var transcodingResultsObservable = schedulers.CreateHotObservable(
                    createFileNotifications(Subscribed + 101)
                                 .Concat(createFileNotifications(Subscribed + 201))
                                 .Concat(createFileNotifications(Subscribed + 301))
                                 .ToArray()
                                 );
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);
            notifications.Setup(n => n.ObserveTranscodingResult()).Returns(transcodingResultsObservable);
            var sut = fixture.Create<NotificationViewModelProducer>();
            //act
            var actual = schedulers.Start(() => sut.ObserveSynchronizedFileCount());
            //assert
            Func<long, IEnumerable<Recorded<Notification<SynchronizedFilesCountViewModel>>>> createExpectedFileNotifications =
                offset => fileNotifications.Select((f, i) => OnNext(offset + i, new SynchronizedFilesCountViewModel(i + 1, 0)));
            var expected = new[]
                {
                    OnNext(Subscribed, SynchronizedFilesCountViewModel.Empty),
                }
                .Concat(createExpectedFileNotifications(Subscribed + 101))
                .Concat(createExpectedFileNotifications(Subscribed + 201))
                .Concat(createExpectedFileNotifications(Subscribed + 301))
                .ToArray();
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }

        [Theory, FileAutoData]
        public void ObserveSynchronizedFileCount_WhenTranscodingIsRunning_AndTranscodingResultsArePushedWithFailures_ShouldReturnCorrectValue(
         [Frozen]TestSchedulers schedulers,
         [Frozen]Mock<ITranscodingNotifications> notifications,
         IFileNotification[] fileNotifications,
         IFileNotification[] failuresNotifications,
         Fixture fixture)
        {
            //arrange          
            var notificationsObservable = schedulers.CreateHotObservable(
                OnNext(201, true));
            var transcodingResultsObservable = schedulers.CreateHotObservable(
                fileNotifications.Select((f, i) => OnNext(203 + i, FileTranscodingResultNotification.CreateSuccess(f)))
                                 .Concat(failuresNotifications.Select((f, i) => OnNext(203 + fileNotifications.Length + i, FileTranscodingResultNotification.CreateFailure(f, new Exception()))))
                                 .ToArray());
            notifications.Setup(n => n.ObserveIsTranscodingRunning()).Returns(notificationsObservable);
            notifications.Setup(n => n.ObserveTranscodingResult()).Returns(transcodingResultsObservable);
            var sut = fixture.Create<NotificationViewModelProducer>();
            //act
            var actual = schedulers.Start(() => sut.ObserveSynchronizedFileCount());
            //assert
            var expected = new[]
            {
                OnNext(Subscribed, SynchronizedFilesCountViewModel.Empty),
            }
            .Concat(fileNotifications.Select((f, i) => OnNext(203 + i, new SynchronizedFilesCountViewModel(i + 1, 0))).ToArray());
            actual.Messages.ShouldAllBeEquivalentTo(expected);
        }
    }
}
