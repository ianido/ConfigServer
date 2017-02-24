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

        private Dictionary<string, List<T>> _watcher;        

        public event ChangeDetection Change;

        protected virtual void OnChange(string groupName, string fileName)
        {
            if (Change != null) Change(groupName, fileName);
        }

        public ConfigWatcher(ILogger<IConfigWatcher> logger)
        {
            _logger = logger;
            _watcher = new Dictionary<string, List<T>>();
            _timer = new Timer(new TimerCallback(Timer_Elapsed), _watcher, Timeout.Infinite, FILEWATCHER_MILLESECONDS);
        }

        private void Timer_Elapsed(object state)
        {
            //Check for file modifications
            _timer.Change(Timeout.Infinite, FILEWATCHER_MILLESECONDS); // Disable the timer;
            foreach (var key in _watcher.Keys.ToList())
            {
                foreach (var w in _watcher[key].ToList())
                {
                    if (w.EnableRaisingEvents == true)
                        w.CheckForChange();
                }
            }
            _timer.Change(FILEWATCHER_MILLESECONDS, FILEWATCHER_MILLESECONDS); // Reenable the timer;
        }

        public void StartMonitoring(string groupName)
        {
            _timer.Change(Timeout.Infinite, FILEWATCHER_MILLESECONDS);
            List<T> watchers = _watcher[groupName];
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = true;
            }
            _timer.Change(FILEWATCHER_MILLESECONDS, FILEWATCHER_MILLESECONDS);
        }

        public void StopMonitoring(string groupName)
        {
            _timer.Change(Timeout.Infinite, FILEWATCHER_MILLESECONDS);
            List<T> watchers = _watcher[groupName];
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = false;
            }
            _timer.Change(FILEWATCHER_MILLESECONDS, FILEWATCHER_MILLESECONDS);
        }

        public void StartMonitoring()
        {
            _timer.Change(Timeout.Infinite, FILEWATCHER_MILLESECONDS);
            foreach (var key in _watcher.Keys)
            {
                foreach (var w in _watcher[key])
                {
                    w.EnableRaisingEvents = true;
                }
            }
            _timer.Change(FILEWATCHER_MILLESECONDS, FILEWATCHER_MILLESECONDS);
        }

        public void StopMonitoring()
        {
            _timer.Change(Timeout.Infinite, FILEWATCHER_MILLESECONDS);
            foreach (var key in _watcher.Keys)
            {
                foreach (var w in _watcher[key])
                {
                    w.EnableRaisingEvents = false;
                }
            }
            _timer.Change(FILEWATCHER_MILLESECONDS, FILEWATCHER_MILLESECONDS);
        }

        public void AddGroupWatcher(string groupName, string[] files)
        {
            if (!_watcher.ContainsKey(groupName))
            {
                List<T> lw = new List<T>();
                foreach (var filename in files)
                {
                    T w = new T();

                    w.Tag = groupName;
                    w.EntityName = filename;
                    w.Changed += W_Changed;
                    w.EnableRaisingEvents = false;
                    lw.Add(w);
                }
                _watcher.Add(groupName, lw.ToList());
            }
            else
                throw new Exception("group already exist: " + groupName);
        }

        public void AddToGroupWatcher(string groupName, string filename)
        {
            if (!_watcher.ContainsKey(groupName))
            {
                _watcher.Add(groupName, new List<T>());
            }
            List<T> fw = _watcher[groupName];

            if (fw.FirstOrDefault(f => f.EntityName == filename) == null)
            {

                T w = new T();
                w.LastWriteDate = new FileInfo(filename).LastWriteTimeUtc;
                w.Tag = groupName;
                w.EntityName = filename;
                w.Changed += W_Changed;
                w.EnableRaisingEvents = false;
                fw.Add(w);
            }
        }

        public void RemoveGroupWatcher(string groupName)
        {
            if (_watcher.ContainsKey(groupName))
            {
                List<T> fw = _watcher[groupName];

                foreach (var w in fw)
                {
                    w.EnableRaisingEvents = false;
                }
                _watcher.Remove(groupName);
            }            
        }

        private void W_Changed(object sender, string entityName)
        {
            OnChange(((IWatcherProvider)sender).Tag, entityName);
        }
    }
}
