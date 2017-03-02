namespace yupisoft.ConfigServer.Core
{

    public delegate void ChangeDetection(object sender, string fileName);
    public interface IConfigWatcher
    {
        event ChangeDetection Change;

        bool IsWatching(string entityName);
        void AddToWatcher(string[] entityNames);
        void AddToWatcher(string entityName);
        void ClearWatcher();
        void StartMonitoring();
        void StopMonitoring();
    }
}