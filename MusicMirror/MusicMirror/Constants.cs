using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror
{
    public static class Constants
    {
        public static class Schedulers
        {
            public const string NotificationsScheduler = "NotificationsScheduler";
        }

        public static class Logging
        {
            public const string DefaultLayout = "[${longdate}] [${level:uppercase=true}] | [${logger}] | ${message} ${exception:format=tostring}";
        }
    }
}
