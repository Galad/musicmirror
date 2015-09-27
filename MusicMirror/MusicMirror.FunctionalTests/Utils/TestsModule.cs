using System;
using Microsoft.Practices.Unity;
using Xunit.Abstractions;
using NLog.Targets;
using NLog;
using NLog.Layouts;

namespace MusicMirror.FunctionalTests.Utils
{
    public class TestsModule : ICompositionModule
    {
        private readonly ITestOutputHelper _output;

        public TestsModule(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Compose(IUnityContainer container)
        {            
            var testOutputTarget = new TestOutputHelperTarget(_output);
            testOutputTarget.Layout = Layout.FromString(Constants.Logging.DefaultLayout);
            var rule = new NLog.Config.LoggingRule("*", LogLevel.Debug, testOutputTarget);                        
            container.RegisterInstance("TestOutputHelper", rule);
        }

        private class TestOutputHelperTarget : TargetWithLayout
        {
            private ITestOutputHelper _output;

            public TestOutputHelperTarget(ITestOutputHelper _output)
            {
                this._output = _output;
            }

            protected override void Write(LogEventInfo logEvent)
            {                
                var s = Layout.Render(logEvent);
                _output.WriteLine(s);
            }
        }
    }
}