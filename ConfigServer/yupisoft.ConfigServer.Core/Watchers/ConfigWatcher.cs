using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace yupisoft.ConfigServer.Core
{
    

    public class ConfigWatcher<T> : IConfigWatcher where T : IWatcherProvider, new()
    {
        private Timer _timer;

        private ILogger _logger;

        private int FILEWATCHER_MILLESECONDS = 2000;

        List<T> _watcher;        

        public event ChangeDetection Change;

        protected virtual void OnChange(string fileName)
        {
            if (Change != null) Change(this, fileName);
        }

        public ConfigWatcher(ILogger<IConfigWatcher> logger)
        {
            _logger = logger;
            _watcher = new List<T>();
            _timer = new Timer(new TimerCallback(Timer_Elapsed), _watcher, Timeout.Infinite, FILEWATCHER_MILLESECONDS);
        }

        private void Timer_Elapsed(object state)
        {
            //Check for file modifications
            _timer.Change(Timeout.Infinite, FILEWATCHER_MILLESECONDS); // Disable the timer;

                foreach (var w in _watcher.ToList())
                {
                    if (w.EnableRaisingEvents == true)
                        w.CheckForChange();
                }

            _timer.Change(FILEWATCHER_MILLESECONDS, FILEWATCHER_MILLESECONDS); // Reenable the timer;
        }

        public void StartMonitoring()
        {
            _timer.Change(Timeout.Infinite, FILEWATCHER_MILLESECONDS);
            foreach (var w in _watcher)
                w.EnableRaisingEvents = true;
            _timer.Change(FILEWATCHER_MILLESECONDS, FILEWATCHER_MILLESECONDS);
        }

        public void StopMonitoring()
        {
            _timer.Change(Timeout.Infinite, FILEWATCHER_MILLESECONDS);
            foreach (var w in _watcher)
                w.EnableRaisingEvents = false;
            _timer.Change(FILEWATCHER_MILLESECONDS, FILEWATCHER_MILLESECONDS);
        }


        public void AddToWatcher(string[] entityNames)
        {
            foreach (var filename in entityNames)
                AddToWatcher(filename);
        }

        public void AddToWatcher(string entityName)
        {
            if (_watcher.FirstOrDefault(f => f.EntityName == entityName) == null)
            {
                T w = new T();
                w.LastWriteDate = new FileInfo(entityName).LastWriteTimeUtc;
                w.EntityName = entityName;
                w.Changed += W_Changed;
                w.EnableRaisingEvents = false;
                _watcher.Add(w);
            }
        }

        public void ClearWatcher()
        {
            _watcher.Clear();
        }

        private void W_Changed(object sender, string entityName)
        {
            OnChange(entityName);
        }
    }
}
