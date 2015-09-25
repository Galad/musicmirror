using Hanno;
using Hanno.Concurrency;
using Hanno.Rx;
using Hanno.Testing.Autofixture;
using MusicMirror.FunctionalTests.Tests;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MusicMirror.FunctionalTests.Utils
{
	public class SingleSchedulers : ISchedulers
	{
		private readonly IPriorityScheduler _scheduler;

		public SingleSchedulers(IPriorityScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public IScheduler CurrentThread => _scheduler;
		public IPriorityScheduler Dispatcher => _scheduler;
		public IScheduler Immediate => _scheduler;
		public IScheduler TaskPool => _scheduler;
		public IPriorityScheduler ThreadPool => _scheduler;

		public void SafeDispatch(Action action)
		{
			action();
		}
	}

	public class TestComposer : Composer
	{
		public TestComposer(string loggingSessionId) : base(
            () => new SingleSchedulers(new SingleSchedulerPriorityScheduler(ThreadPoolScheduler.Instance)),
                  new LoggingComposer(loggingSessionId))
		{ }
	}

	#region Customization
	public class SpecificationCustomization : ICustomization
	{
		public const string ReferenceTestFileRootFolder = "TestFiles";
		public const string TestFileRootFolder = "Tests";

		public void Customize(IFixture fixture)
		{
			fixture.Register(() =>
			{
                var sessionId = Guid.NewGuid().ToString();
                var uniqueTestFolder = Path.Combine(Environment.CurrentDirectory, TestFileRootFolder, sessionId);
				var testFilesFolder = Path.Combine(Environment.CurrentDirectory, ReferenceTestFileRootFolder);
                Debug.WriteLine($"DebugTestFolder is {uniqueTestFolder}");
				return new TestContext(new TestComposer(sessionId), new TestFilesRepository(testFilesFolder), uniqueTestFolder);
			});
			fixture.Freeze<TestContext>();
		}
	}

	public class SpecificationCompositeCustomization : CompositeCustomization
	{
		public SpecificationCompositeCustomization() : base(new SpecificationCustomization())
		{
		}
	}

	public class SpecificationAutoDataAttribute : AutoDataAttribute
	{
		public SpecificationAutoDataAttribute() : base(new Fixture().Customize(new SpecificationCompositeCustomization()))
		{
		}
	}

	public class SpecificationInlineAutoDataAttribute : CompositeDataAttribute
	{
		public SpecificationInlineAutoDataAttribute(params object[] values) : base(new InlineDataAttribute(values), new SpecificationAutoDataAttribute())
		{
		}
	}
	#endregion
}
