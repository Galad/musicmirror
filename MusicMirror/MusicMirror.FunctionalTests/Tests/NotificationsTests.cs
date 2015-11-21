using FluentAssertions;
using Hanno.Testing.Autofixture;
using Hanno.ViewModels;
using Microsoft.Reactive.Testing;
using MusicMirror.FunctionalTests.Utils;
using MusicMirror.ViewModels;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MusicMirror.FunctionalTests.Tests
{
    [Trait("TestLevel", "Functional")]
    public class NotificationsTests : IDisposable, IAsyncLifetime
    {
        private readonly IFixture _fixture;
        private readonly TestContext _context;

        public NotificationsTests(ITestOutputHelper output)
        {
            _fixture = new Fixture().Customize(new SpecificationCustomization(output));
            _context = _fixture.Create<TestContext>();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task InitializeAsync()
        {
            await _context.Load(CancellationToken.None);
        }

        public Task DisposeAsync()
        {
            return Task.FromResult(true);
        }

        [Fact]
        public void Synchronization_ShouldBeDisable()
        {
            _context.ViewModel.IsSynchronizationEnabled.Value.Should().BeFalse();
        }

        [Fact]
        public async Task WhenIEnableTheSynchronization_TheSynchronizationShouldBeEnabled()
        {
            //arrange
            var observable = _context.ViewModel.IsSynchronizationEnabled.Replay();
            //act
            using (observable.Connect())
            {
                _context.ViewModel.EnableSynchronizationCommand.Execute(null);
                var actual = await observable.Skip(1).Take(1).ToTask(TestContextUtils.CreateShortTimedOutCancellationToken());
                //assert
                actual.Should().BeTrue();
            }
        }

        [Fact]
        public async Task GivenTheSynchronizationIsEnabled_WhenIDisableTheSynchronization_TheSynchronizationShouldBeDisabled()
        {
            //arrange
            _context.ViewModel.EnableSynchronizationCommand.Execute(null);
            var observable = _context.ViewModel.IsSynchronizationEnabled.Replay();
            //act
            using (observable.Connect())
            {
                _context.ViewModel.DisableSynchronizationCommand.Execute(null);
                var actual = await observable.Skip(1).Take(1).ToTask(TestContextUtils.CreateShortTimedOutCancellationToken());
                //assert
                actual.Should().BeFalse();
            }
        }

        [Fact]
        public async Task WhenSynchronizationIsDisabled_ThenTheSynchronizationStatusShouldBeEmpty()
        {
            //act			
            var actual = await _context.ViewModel.SynchronizedFileCount.Take(1).ToTask(TestContextUtils.CreateShortTimedOutCancellationToken());
            //assert			
            actual.ShouldBeEquivalentTo(SynchronizedFilesCountViewModel.Empty);
        }

        [Fact]
        public async Task WhenSynchronizationIsEnabled_AndThereIsNoFileInTheTargetFolder_ThenTheSynchronizationStatusShouldBeEmpty()
        {
            //act			
            _context.ViewModel.EnableSynchronizationCommand.Execute(null);
            await Task.Delay(1.Seconds());
            var actual = await _context.ViewModel.SynchronizedFileCount.Take(1).ToTask(TestContextUtils.CreateShortTimedOutCancellationToken()).ConfigureAwait(false);
            //assert			
            actual.ShouldBeEquivalentTo(SynchronizedFilesCountViewModel.Empty);
        }

        [Fact]
        public async Task WhenSynchronizationIsDisabled_ThenTheTranscodingStatusShoulReturnFalse()
        {
            //act			
            var actual = await _context.ViewModel.IsTranscodingRunning.Take(1).ToTask(TestContextUtils.CreateShortTimedOutCancellationToken());
            //assert			
            actual.Should().BeFalse();
        }

        [Fact]
        public async Task WhenSynchronizationIsEnabled_AndThereIsNoFileInTheTargetFolder_ThenTheTranscodingStatusShoulReturnFalse()
        {
            //act			
            _context.ViewModel.EnableSynchronizationCommand.Execute(null);
            await Task.Delay(1.Seconds());
            var actual = await _context.ViewModel.IsTranscodingRunning.Take(1).ToTask(TestContextUtils.CreateShortTimedOutCancellationToken());
            //assert			
            actual.Should().BeFalse();
        }

        public static IEnumerable<object[]> SynchronizationEnabledTestFiles
        {
            get
            {
                yield return new object[] { new[] { TestFilesConstants.Flac.SourceNormalFile1 } };
                yield return new object[] { new[] { TestFilesConstants.MP3.SourceNormalFile1 } };
                yield return new object[] { new[] { TestFilesConstants.Flac.SourceNormalFile1, TestFilesConstants.MP3.SourceNormalFile2 } };
                yield return new object[] { new[] { TestFilesConstants.Flac.SourceNormalFile1, TestFilesConstants.MP3.SourceNormalFile2, TestFilesConstants.MP3.SourceFileWithWrongDisplayedDuration } };
            }
        }

        [Fact]
        public async Task WhenSynchronizationIsEnabled_AndThereIsFilesInTheTargetFolder_ThenIsTranscodingRunningShouldReturnTrue()
        {
            //arrange
            _context.SourceDirectorySetup()
                    .WithFile(TestFilesConstants.MP3.SourceNormalFile1);
            var task = _context.ViewModel.IsTranscodingRunning.Skip(1).Take(1).ToTask(TestContextUtils.CreateShortTimedOutCancellationToken());
            _context.ViewModel.EnableSynchronizationCommand.Execute(null);
            //act
            var actual = await task;
            //assert
            actual.Should().BeTrue();
        }

        [Theory, MemberData("SynchronizationEnabledTestFiles")]
        public async Task WhenSynchronizationIsEnabled_AndThereIsFilesInTheTargetFolder_ThenTheSynchronizationStatusYieldTheCorrectValue(
            string[] files)
        {
            //act			
            files.Aggregate(_context.SourceDirectorySetup(), (s, f) => s.WithFile(f));
            var scheduler = new TestScheduler();
            var observer = scheduler.CreateObserver<SynchronizedFilesCountViewModel>();
            using (_context.ViewModel.SynchronizedFileCount.Subscribe(observer))
            {
                var task = _context.WaitUntilTranscodingComplete(TestContextUtils.CreateLongTimedOutCancellationToken());
                _context.ViewModel.EnableSynchronizationCommand.Execute(null);
                await task;
            }
            var actual = observer.Values().ToArray();
            //assert
            var expected = new[] { SynchronizedFilesCountViewModel.Empty }
                            .Concat(files.Select((f, i) => new SynchronizedFilesCountViewModel(i, files.Length)))
                            .Concat(new[] { new SynchronizedFilesCountViewModel(files.Length, files.Length) });
            actual.ShouldAllBeEquivalentTo(expected);
        }

        [Fact]
        public async Task WhenSynchronizationIsEnabled_AndThereIsFilesInTheTargetFolder_WhenStoppingDuringTranscodingAndRestartingSynchronization_ThenTheSynchronizationStatusYieldTheCorrectValue(
            )
        {
            //act			
            _context.SourceDirectorySetup()
                    .WithFile(TestFilesConstants.Flac.SourceNormalFile1)
                    .WithFile(TestFilesConstants.MP3.SourceNormalFile1);
            var scheduler = new TestScheduler();
            var observer = scheduler.CreateObserver<SynchronizedFilesCountViewModel>();
            var task = _context.WaitUnitFirstFileIsFound(TestContextUtils.CreateShortTimedOutCancellationToken());
            _context.ViewModel.EnableSynchronizationCommand.Execute(null);
            await task;
            _context.ViewModel.DisableSynchronizationCommand.Execute(null);
            using (_context.ViewModel.SynchronizedFileCount.Subscribe(observer))
            {
                var task2 = _context.WaitUntilTranscodingComplete(TestContextUtils.CreateLongTimedOutCancellationToken());
                _context.ViewModel.EnableSynchronizationCommand.Execute(null);
                await task2;
            }
            var actual = observer.Values().ToArray();
            //assert
            var expected = new[] { SynchronizedFilesCountViewModel.Empty }
            .Concat(new[] {
                new SynchronizedFilesCountViewModel(0, 2),
                new SynchronizedFilesCountViewModel(1, 2),
                new SynchronizedFilesCountViewModel(2, 2)
            });
            actual.ShouldAllBeEquivalentTo(expected);
        }

        [Theory, MemberData("SynchronizationEnabledTestFiles")]
        public async Task WhenSynchronizationIsEnabled_AndThereIsFilesInTheTargetFolder_WhenStoppingAndRestartingSynchronization_ThenTheSynchronizationStatusYieldTheCorrectValue(
           string[] files)
        {
            //act			
            files.Aggregate(_context.SourceDirectorySetup(), (s, f) => s.WithFile(f));
            var scheduler = new TestScheduler();
            var observer = scheduler.CreateObserver<SynchronizedFilesCountViewModel>();
            using (_context.ViewModel.SynchronizedFileCount.Subscribe(observer))
            {
                var task = _context.WaitUntilTranscodingComplete(TestContextUtils.CreateLongTimedOutCancellationToken());
                _context.ViewModel.EnableSynchronizationCommand.Execute(null);
                await task;
                _context.ViewModel.DisableSynchronizationCommand.Execute(null);
                var task2 = _context.WaitUntilTranscodingComplete(TestContextUtils.CreateLongTimedOutCancellationToken());
                _context.ViewModel.EnableSynchronizationCommand.Execute(null);
                await task2;
            }
            var actual = observer.Values().ToArray();
            //assert
            var expected = new[] { SynchronizedFilesCountViewModel.Empty }
            .Concat(Enumerable.Repeat(
                            files.Select((f, i) => new SynchronizedFilesCountViewModel(i, files.Length))
                                 .Concat(new[] { new SynchronizedFilesCountViewModel(files.Length, files.Length) }),
                            2)
                .SelectMany(t => t));
            actual.ShouldAllBeEquivalentTo(expected);
        }

        //[Fact]
        //public async Task WhenIStartTheSynchronization_IShouldReceiveANotification()
        //{
        //	////arrange
        //	//var observable = _context.SynchronizationNotifications
        //	//						.ObserveSynchronizationNotifications()
        //	//						.Take(1)
        //	//						.Replay();
        //	////act
        //	//using (observable.Connect())
        //	//{
        //	//	var actual = await observable.ToTask(TestContextUtils.CreateTimedOutCancellationToken());
        //	//	//assert
        //	//	actual.Should().NotBeNull();
        //	//}
        //}

        //[Fact]
        //public async Task WhenIStopTheSynchronization_IShouldReceiveANotification()
        //{
        //	////arrange
        //	//var observable = _context.SynchronizationNotifications
        //	//						 .ObserveSynchronizationNotifications()
        //	//						 .SelectMany(o => o.Materialize()
        //	//									   .Where(n => n.Kind == System.Reactive.NotificationKind.OnCompleted))								     
        //	//						 .Take(1)
        //	//						 .Replay();
        //	////act
        //	//using (observable.Connect())
        //	//using(_context.SynchronizationController.Enable())
        //	//{
        //	//	var actual = await observable.ToTask(TestContextUtils.CreateTimedOutCancellationToken());
        //	//	//assert
        //	//	actual.Should().NotBeNull();
        //	//}						
        //}

        //[InlineData]
        //public async Task WhenIStartAndStopTheSynchronizationMultipleTimes_IShouldReceiveANotification()
        //{
        //	////arrange			
        //	//var observable = _context.SynchronizationNotifications
        //	//						.ObserveSynchronizationNotifications()
        //	//						.Take(1)
        //	//						.Replay();
        //	////act
        //	//using (observable.Connect())
        //	//using (_context.SynchronizationController.Enable())
        //	//{
        //	//	var actual = await observable.ToTask(TestContextUtils.CreateTimedOutCancellationToken());
        //	//	//assert
        //	//	actual.Should().NotBeNull();
        //	//}
        //}
    }
}
