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
    public class LoggingComposer
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
            container.RegisterType<LoggingConfiguration>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => CreateConfiguration()));
            container.RegisterType<LogFactory>(
                new ContainerControlledLifetimeManager(), 
                new InjectionFactory(c => new LogFactory(c.Resolve<LoggingConfiguration>())));
            container.RegisterType<Func<string, ILogger>>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => CreateLoggerFactory(c))
                );
        }

        private Func<string, ILogger> CreateLoggerFactory(IUnityContainer c)
        {
            return (name) => c.Resolve<LogFactory>().GetLogger(name);
        }

        private LoggingConfiguration CreateConfiguration()
        {
            var config = new LoggingConfiguration();
            var debuggerTarget = new DebuggerTarget();
            var fileTarget = new FileTarget() { FileName = "${basedir}/Logs/" + SessionId + "/${level}.log" };
            var rule1 = new LoggingRule("*", LogLevel.Debug,  debuggerTarget);
            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule1);
            config.LoggingRules.Add(rule2);
            return config;
        }
    }
}
