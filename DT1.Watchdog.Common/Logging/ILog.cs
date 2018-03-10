using System;
using System.Collections.Generic;
using System.Text;

namespace DT1.Watchdog.Common.Logging
{
    public interface ILog
    {
        void Debug( string format, params object[] parameters);
    }
}
