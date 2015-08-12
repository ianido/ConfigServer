
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    public class NLogLoggerService : ILoggerService
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public void Log(string message, EventType eType)
        {
            switch (eType)
            {
                case EventType.Info : 
                    {
                        _logger.Info(message);
                    }break;
                case EventType.Debug:
                    {
                        _logger.Debug(message);
                    } break;
                case EventType.Error:
                    {
                        _logger.Error(message);
                    } break;
                case EventType.Fatal:
                    {
                        _logger.Fatal(message);
                    } break;
                case EventType.Warn:
                    {
                        _logger.Warn(message);
                    } break;                
            }
        }

        public void Log(string message, Exception ex, EventType eType)
        {
            switch (eType)
            {
                case EventType.Info:
                    {
                        _logger.InfoException(message, ex);
                    } break;
                case EventType.Debug:
                    {
                        _logger.DebugException(message, ex);
                    } break;
                case EventType.Error:
                    {
                        _logger.ErrorException(message, ex);
                    } break;
                case EventType.Fatal:
                    {
                        _logger.FatalException(message, ex);
                    } break;
                case EventType.Warn:
                    {
                        _logger.WarnException(message, ex);
                    } break;
            }
        }
    }
}
