namespace yupisoft.ConfigServer.Core
{

    public delegate void ChangeDetection(string groupName, string fileName);
    public interface IConfigWatcher
    {
        event ChangeDetection Change;
        void AddGroupWatcher(string groupName, string[] files);
        void AddToGroupWatcher(string groupName, string filename);
        void RemoveGroupWatcher(string groupName);
        void StartMonitoring();
        void StartMonitoring(string groupName);
        void StopMonitoring();
        void StopMonitoring(string groupName);
    }
}