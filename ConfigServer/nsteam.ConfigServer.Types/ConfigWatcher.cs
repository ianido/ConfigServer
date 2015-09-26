using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace nsteam.ConfigServer.Types
{
    public class MyFileSystemWatcher : FileSystemWatcher
    {
        public string Tag { get; set; }

        public MyFileSystemWatcher(string path, string filter) : base(path, filter) { }
    }

    public class SimpleFileSystemWatcher
    {
        public string Tag { get; set; }

        public string FilePath { get; set; }

        public bool EnableRaisingEvents { get; set; }

        public DateTime LastWriteDate { get; set; }

        public event FileSystemEventHandler Changed;

        public void CheckForChange()
        {
            FileInfo fi = new FileInfo(FilePath);
            if (fi.LastWriteTimeUtc != LastWriteDate)
            {
                EnableRaisingEvents = false;
                Changed(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, fi.DirectoryName, fi.Name));
                LastWriteDate = fi.LastWriteTimeUtc;
                EnableRaisingEvents = true;
            }
        }

    }


    public class ConfigWatcher
    {
        private Timer timer;

        private Dictionary<string, List<SimpleFileSystemWatcher>> _watcher;

        public delegate void ChangeDetection(string groupName, string fileName);

        public event ChangeDetection Change;

        protected virtual void OnChange(string groupName, string fileName)
        {
            if (Change != null) Change(groupName, fileName);
        }

        public ConfigWatcher()
        {
            _watcher = new Dictionary<string, List<SimpleFileSystemWatcher>>();

            timer = new Timer(2000);
            timer.Enabled = false;
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Check for file modifications
            timer.Enabled = false;
            foreach (var key in _watcher.Keys.ToList())
            {
                foreach (var w in _watcher[key].ToList())
                {
                    if (w.EnableRaisingEvents == true)
                        w.CheckForChange();
                }
            }
            timer.Enabled = true;
        }

        public void StartMonitoring(string groupName)
        {
            timer.Enabled = false;
            List<SimpleFileSystemWatcher> watchers = _watcher[groupName];
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = true;
            }
            timer.Enabled = true;
        }

        public void StopMonitoring(string groupName)
        {
            timer.Enabled = false;
            List<SimpleFileSystemWatcher> watchers = _watcher[groupName];
            foreach (var w in watchers)
            {
                w.EnableRaisingEvents = false;
            }
            timer.Enabled = true;
        }

        public void StartMonitoring()
        {
            timer.Enabled = false;
            foreach (var key in _watcher.Keys)
            {
                foreach (var w in _watcher[key])
                {
                    w.EnableRaisingEvents = true;
                }
            }
            timer.Enabled = true;
        }

        public void StopMonitoring()
        {
            timer.Enabled = false;
            foreach (var key in _watcher.Keys)
            {
                foreach (var w in _watcher[key])
                {
                    w.EnableRaisingEvents = false;
                }
            }
            timer.Enabled = true;
        }

        public void AddGroupWatcher(string groupName, string[] files)
        {
            if (!_watcher.ContainsKey(groupName))
            {
                List<SimpleFileSystemWatcher> lw = new List<SimpleFileSystemWatcher>();
                foreach (var filename in files)
                {
                    SimpleFileSystemWatcher w = new SimpleFileSystemWatcher();

                    w.Tag = groupName;
                    w.FilePath = filename;
                    w.Changed += W_Changed;
                    w.EnableRaisingEvents = false;
                    lw.Add(w);
                }
                _watcher.Add(groupName, lw.ToList());
            }
            else
                throw new ApplicationException("group already exist: " + groupName);
        }



        public void AddToGroupWatcher(string groupName, string filename)
        {
            if (!_watcher.ContainsKey(groupName))
            {
                _watcher.Add(groupName, new List<SimpleFileSystemWatcher>());
            }
            List<SimpleFileSystemWatcher> fw = _watcher[groupName];

            if (fw.FirstOrDefault(f => f.FilePath == filename) == null)
            {

                SimpleFileSystemWatcher w = new SimpleFileSystemWatcher();
                w.LastWriteDate = new FileInfo(filename).LastWriteTimeUtc;
                w.Tag = groupName;
                w.FilePath = filename;
                w.Changed += W_Changed;
                w.EnableRaisingEvents = false;
                fw.Add(w);
            }
        }

        public void RemoveGroupWatcher(string groupName)
        {
            if (_watcher.ContainsKey(groupName))
            {
                List<SimpleFileSystemWatcher> fw = _watcher[groupName];

                foreach (var w in fw)
                {
                    w.EnableRaisingEvents = false;
                }
                _watcher.Remove(groupName);
            }            
        }

        private void W_Changed(object sender, FileSystemEventArgs e)
        {
            OnChange(((SimpleFileSystemWatcher)sender).Tag, e.FullPath);
        }
    }
}
