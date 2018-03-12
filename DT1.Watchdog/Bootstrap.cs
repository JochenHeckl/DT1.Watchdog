using Autofac;
using DT1.Watchdog.Common;
using DT1.Watchdog.Common.Logging;

namespace DT1.Watchdog
{
    public static class Bootstrap
    {
        public static IContainer Container
        {
            get;
            set;
        }
    }
}