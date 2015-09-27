using Hanno.Extensions;
using Microsoft.Practices.Unity;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
    public class LoggingComposer : ICompositionModule
    {
        private readonly string _sessionId;

        public LoggingComposer() : this(Guid.NewGuid().ToString()) { }

        public LoggingComposer(string sessionId)
        {
            _sessionId = sessionId;
        }

        public string SessionId
        {
            get
            {
                return _sessionId;
            }
        }

        public void Compose(IUnityContainer container)
        {
            RegisterDebuggerTarget(container);
            RegisterFileTarget(container);
            container.RegisterType<LoggingConfiguration>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => CreateConfiguration(c)));
            container.RegisterType<LogFactory>(
                new ContainerControlledLifetimeManager(), 
                new InjectionFactory(c => new LogFactory(c.Resolve<LoggingConfiguration>())));
            container.RegisterType<Func<string, ILogger>>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => CreateLoggerFactory(c))
                );
        }

        private void RegisterFileTarget(IUnityContainer container)
        {
            var fileTarget = new FileTarget() { FileName = "${basedir}/Logs/" + SessionId + "/${level}.log" };
            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            container.RegisterInstance("File", rule2);
        }

        private void RegisterDebuggerTarget(IUnityContainer container)
        {
            var debuggerTarget = new DebuggerTarget();
            var rule1 = new LoggingRule("*", LogLevel.Debug,  debuggerTarget);
            container.RegisterInstance("Debugger", rule1);
        }

        private Func<string, ILogger> CreateLoggerFactory(IUnityContainer c)
        {
            return (name) => c.Resolve<LogFactory>().GetLogger(name);
        }

        private LoggingConfiguration CreateConfiguration(IUnityContainer container)
        {
            var config = new LoggingConfiguration();
            config.LoggingRules.AddRange(container.ResolveAll<LoggingRule>());
            return config;
        }
    }
}
