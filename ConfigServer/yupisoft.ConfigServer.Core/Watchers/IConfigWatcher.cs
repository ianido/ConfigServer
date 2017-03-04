namespace yupisoft.ConfigServer.Core
{

    public delegate void ChangeDetection(object sender, string entityName);
    public interface IConfigWatcher
    {
        event ChangeDetection Change;
        string[] GetEntities();

        bool IsWatching(string entityName);
        void AddToWatcher(string entityName, string connection);
        void ClearWatcher();
        void StartMonitoring();
        void StopMonitoring();
    }
}