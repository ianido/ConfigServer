using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    public enum EventType
    {
        Info,
        Warn,
        Error,
        Fatal,
        Debug,
    }

    public interface ILoggerService
    {
        void Log(string message, EventType eType);
        void Log(string message, Exception ex, EventType eType);
    }
}
