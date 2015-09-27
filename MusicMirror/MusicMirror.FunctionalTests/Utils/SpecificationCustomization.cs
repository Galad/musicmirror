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
using Xunit.Abstractions;

namespace MusicMirror.FunctionalTests.Utils
{
    public sealed class TestComposer : Composer
	{
		public TestComposer(string sessionId, ITestOutputHelper output) : base(
                  new CompositeCompositionModule(
                      new LoggingModule(sessionId),
                      new TestsModule(output),
                      new TestSchedulersModule()))
		{
            SettingsFileName = "Settings" + sessionId;
        }
	}

	#region Customization
	public sealed class SpecificationCustomization : ICustomization
	{
		public const string ReferenceTestFileRootFolder = "TestFiles";
		public const string TestFileRootFolder = "Tests";
        private readonly ITestOutputHelper _output;

        public SpecificationCustomization(ITestOutputHelper output)
        {
            if (output == null) throw new ArgumentNullException("output");
            _output = output;
        }

        public void Customize(IFixture fixture)
		{
			fixture.Register(() =>
			{
                var sessionId = Guid.NewGuid().ToString();
                _output.WriteLine($"Session is {sessionId}");
                var uniqueTestFolder = Path.Combine(Environment.CurrentDirectory, TestFileRootFolder, sessionId);
				var testFilesFolder = Path.Combine(Environment.CurrentDirectory, ReferenceTestFileRootFolder);
                Debug.WriteLine($"DebugTestFolder is {uniqueTestFolder}");
				return new TestContext(new TestComposer(sessionId, _output), new TestFilesRepository(testFilesFolder), uniqueTestFolder);
			});
			fixture.Freeze<TestContext>();
		}
	}

	public class SpecificationCompositeCustomization : CompositeCustomization
	{
		public SpecificationCompositeCustomization(ITestOutputHelper output) : base(new SpecificationCustomization(output))
		{
		}
	}
	#endregion
}
