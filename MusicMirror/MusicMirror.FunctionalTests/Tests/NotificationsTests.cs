using FluentAssertions;
using MusicMirror.FunctionalTests.Utils;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MusicMirror.FunctionalTests.Tests
{
	 [Trait("TestLevel", "Functional")]
	public class NotificationsTests : IDisposable
	{
		private readonly IFixture _fixture;
		private readonly TestContext _context;

		public NotificationsTests()
		{
			_fixture = new Fixture().Customize(new SpecificationCustomization());
			_context = _fixture.Create<TestContext>();
		}

		public void Dispose()
		{
			_context.Dispose();
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
