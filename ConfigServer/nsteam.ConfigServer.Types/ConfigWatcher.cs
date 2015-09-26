using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    public class MyFileSystemWatcher : FileSystemWatcher
    {
        public string Tag { get; set; }

        public MyFileSystemWatcher(string path, string filter) : base(path, filter) { }
    }

    public class ConfigWatcher
    {
        private Dictionary<string, MyFileSystemWatcher[]> _watcher;

        public delegate void ChangeDetection(string groupName, string fileName);

        public event ChangeDetection Change;

        protected virtual void OnChange(string groupName, string fileName)
        {
            if (Change != null) Change(groupName, fileName);
        }

        public ConfigWatcher()
        {
            _watcher = new Dictionary<string, MyFileSystemWatcher[]>();
        }

        public void StartMonitoring(string groupName)
        {
            FileSystemWatcher[] watchers = _watcher[groupName];
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = true;
            }
        }

        public void StopMonitoring(string groupName)
        {
            FileSystemWatcher[] watchers = _watcher[groupName];
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = false;
            }
        }

        public void StartMonitoring()
        {
            foreach (var key in _watcher.Keys)
            {
                foreach (var w in _watcher[key])
                {
                    w.EnableRaisingEvents = true;
                }
            }
        }

        public void StopMonitoring()
        {
            foreach (var key in _watcher.Keys)
            {
                foreach (var w in _watcher[key])
                {
                    w.EnableRaisingEvents = false;
                }
            }
        }

        public void AddGroupWatcher(string groupName, string[] files)
        {
            if (!_watcher.ContainsKey(groupName))
            {
                List<MyFileSystemWatcher> lw = new List<MyFileSystemWatcher>();
                foreach (var filename in files)
                {
                    MyFileSystemWatcher w = new MyFileSystemWatcher(Path.GetDirectoryName(filename), Path.GetFileName(filename));
                    w.NotifyFilter = NotifyFilters.LastWrite;
                    w.IncludeSubdirectories = false;
                    w.Tag = groupName;
                    w.Changed += W_Changed;
                    w.EnableRaisingEvents = false;
                    lw.Add(w);
                }
                _watcher.Add(groupName, lw.ToArray());
            }
            else
                throw new ApplicationException("group already exist: " + groupName);
        }

        public void RemoveGroupWatcher(string groupName)
        {
            if (_watcher.ContainsKey(groupName))
            {
                MyFileSystemWatcher[] fw = _watcher[groupName];

                foreach (var w in fw)
                {
                    w.EnableRaisingEvents = false;
                    w.Dispose();
                }
                _watcher.Remove(groupName);
            }
            else
                throw new ApplicationException("group do not exist: " + groupName);
        }

        private void W_Changed(object sender, FileSystemEventArgs e)
        {
            OnChange(((MyFileSystemWatcher)sender).Tag, e.FullPath);
        }
    }
}
