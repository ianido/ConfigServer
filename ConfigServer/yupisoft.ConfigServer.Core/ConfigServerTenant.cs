using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using yupisoft.ConfigServer.Core.Stores;
using yupisoft.ConfigServer.Core.Watchers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using yupisoft.ConfigServer.Core.Cluster;
using yupisoft.ConfigServer.Core.Utils;
using yupisoft.ConfigServer.Core.Services;
using yupisoft.ConfigServer.Core.Hooks;

namespace yupisoft.ConfigServer.Core
{
    public delegate void LoadTenantEventHandler(ConfigServerTenant tenant, JToken dataToken, bool startingUp);

    public class LoadDataResult
    {
        public string DataHash { get; set; }
        public EntityChanges[] Changes { get; set; }
    }
    public class EntityChanges
    {
        public string entity { get; set; }
        public JToken diffToken { get; set; }
    }
    public class ConfigServerTenant
    {
        private object _lock = new object();
        public TenantConfigSection TenantConfig { get; private set; }
        public IStoreProvider Store { get; }
        public string Id { get { return TenantConfig.Id; } }
        public string Name { get { return TenantConfig.Name; } }
        public bool EnableServiceDiscovery { get { return TenantConfig.EnableServiceDiscovery; } }
        public bool Encrypted { get { return TenantConfig.Encrypted; } }
        public string StartEntityName { get { return Store.StartEntityName; } }
        public string ACLEntityName { get { return Store.ACLEntityName; } }
        public JToken Token { get; private set; }
        public JTenantACL ACL {
            get {
                if (ACLToken != null) return ACLToken.ToObject<JTenantACL>();
                return null;
            }
        }
        public JToken ACLToken { get; private set; }
        public Dictionary<string, JToken> RawTokens { get; private set; }
        public Dictionary<string, Service> Services { get; private set; }
        public Dictionary<string, Hook> Hooks { get; private set; }

        public string DataHash {
            get
            {
                string allData = "";
                foreach(var v in RawTokens)
                    allData += v.Value.ToString(Newtonsoft.Json.Formatting.None);                    
                ulong dataHash = StringHandling.CalculateHash(allData.ToString());
                return dataHash.ToString();
            }
        }
        private ILogger _logger { get; set; }
        private ConfigServerServices _serviceManager { get; set; }

        public event LoadTenantEventHandler StartLoadTenantData;
        public event LoadTenantEventHandler EndLoadTenantData;

        protected virtual void OnStartLoadTenantData(JToken dataToken, bool startingUp)
        {
            StartLoadTenantData?.Invoke(this, dataToken, startingUp);
        }
        protected virtual void OnEndLoadTenantData(JToken dataToken, bool startingUp)
        {
            EndLoadTenantData?.Invoke(this, dataToken, startingUp);
        }

        public ConfigServerTenant(TenantConfigSection tenantConfig, IHostingEnvironment env, ILogger logger)
        {
            _logger = logger;
            RawTokens = new Dictionary<string, JToken>();
            Services = new Dictionary<string, Service>();
            Hooks = new Dictionary<string, Hook>();
            TenantConfig = tenantConfig;

            switch (tenantConfig.Store.Provider)
            {
                 
                case "FileStoreProvider":
                    {
                        if (tenantConfig.Store.Connection.Contains("$ContentRoot") && env != null)
                            tenantConfig.Store.Connection = tenantConfig.Store.Connection.Replace("$ContentRoot", env.ContentRootPath);
                        Store = new FileStoreProvider(tenantConfig.Store, new ConfigWatcher<FileWatcherProvider>(logger), logger, this);
                    }
                    break;
                case "SqlServerStoreProvider":
                    {
                        Store = new SqlServerStoreProvider(tenantConfig.Store, new ConfigWatcher<SqlServerWatcherProvider>(logger), logger, this);
                    }
                    break;
                case "MongoStoreProvider":
                    {
                        Store = new MongoStoreProvider(tenantConfig.Store, new ConfigWatcher<MongoWatcherProvider>(logger), logger, this);
                    }
                    break;
            }
        }

        private void DiscoverServices(JToken token)
        {
            lock (token)
            {
                JToken[] services = token.SelectTokens("$..$service").ToArray();
                Services.Clear();
                foreach (var s in services)
                {
                    JServiceConfig service = s.Parent.Parent.ToObject<JServiceConfig>();
                    if (Services.ContainsKey(service.Id))
                        _logger.LogError("Service Id:(" + service.Id + ") already exist; Service Name: " + service.Name);
                    else
                    {
                        Services.Add(service.Id, new Service(service, _logger));
                        _logger.LogTrace("Registering Service " + service.Name + "(" + service.Id + ")");
                    }
                }

            }
        }

        private void DiscoverHooks(JToken token)
        {
            lock (token)
            {
                JToken[] hooks = token.SelectTokens("$..$hook").ToArray();
                
                List<JHookConfig> jhooks = new List<JHookConfig>();

                foreach (var s in hooks)
                {                    
                    JHookConfig hook = s.Parent.Parent.ToObject<JHookConfig>();
                    jhooks.Add(hook);
                    if (Hooks.ContainsKey(hook.Id))
                    {
                        Hooks[hook.Id].UpdateFrom(hook);
                        //_logger.LogError("Hook Id:(" + hook.Id + ") already exist.");
                    }
                    else
                    {
                        Hooks.Add(hook.Id, Hook.CreateHook(hook, _logger, this));
                        _logger.LogTrace("Registering Hook " + hook.HookType + "(" + hook.Id + ")");
                    }
                }

                List<string> toDelete = new List<string>();

                foreach (var s in Hooks)
                {
                    bool exist = jhooks.Any(t => t.Id == s.Key);
                    if (!exist) toDelete.Add(s.Key);
                }

                foreach (var hookId in toDelete)
                {
                    Hooks.Remove(hookId);
                }
            }
        }

        public LoadDataResult Load(bool startingUp)
        {
            List<EntityChanges> tchanges = new List<EntityChanges>();
            Store.Watcher.StopMonitoring();
            Store.Watcher.ClearWatcher();
            OnStartLoadTenantData(Token, startingUp);

            Token = Store.Get(StartEntityName);
            if (!string.IsNullOrEmpty(ACLEntityName)) ACLToken = Store.GetRaw(ACLEntityName);

            DiscoverServices(Token);
            DiscoverHooks(Token);

            var newRawTokens = new Dictionary<string, JToken>();
            if (startingUp)
            {
                newRawTokens.Add(StartEntityName, Store.GetRaw(StartEntityName));
                if (!string.IsNullOrEmpty(ACLEntityName)) newRawTokens.Add(ACLEntityName, ACLToken);
            }
            else
            {
                foreach (var entity in Store.Watcher.GetEntities())
                {
                    var rawToken = Store.GetRaw(entity);
                    if (!startingUp && RawTokens.ContainsKey(entity))
                    {
                        var previousToken = RawTokens[entity];
                        var jsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch();
                        JToken diffToken = jsonDiff.Diff(previousToken, rawToken);
                        if (diffToken != null) tchanges.Add(new EntityChanges() { entity = entity, diffToken = diffToken });
                    }
                    newRawTokens.Add(entity, rawToken);
                }
            }
            RawTokens.Clear();
            RawTokens = newRawTokens;

            Store.Watcher.StartMonitoring();
            OnEndLoadTenantData(Token, startingUp);

            return new LoadDataResult() { Changes = tchanges.ToArray(), DataHash = this.DataHash };
        }

        public JNode GetRaw(string path, string entityName)
        {
            if (Token == null) throw new Exception("Tenant: " + Id + " not loaded.");
            lock (_lock)
            {
                if (entityName == "@default") entityName = Store.StartEntityName;
                JToken selToken = RawTokens[entityName].SelectToken(path);
                if (selToken == null) return new JNode(path, "{}", entityName);
                var result = selToken.ToObject<JToken>();
                return new JNode(path, result, entityName);
            }
        }

        public T Get<T>(string path)
        {
            if (Token == null) throw new Exception("Tenant: " + Id + " not loaded.");

            lock (_lock)
            {
                JToken selToken = Token.SelectToken(path);
                if (selToken == null) return default(T);
                var result = selToken.ToObject<T>();
                return result;
            }
        }

        public bool Set(JNode newToken, string tenantId, bool stopMonitoring)
        {            
            if (Token == null)
            {
                _logger.LogWarning("ApplyUpdate: Tenant: " + Id + " not loader.");
                new Exception("ApplyUpdate: Tenant: " + Id + " not loaded.");
            }

            lock (_lock)
            {
                if (Store.Watcher.IsWatching(newToken.Entity))
                {
                    JToken rawToken = Store.GetRaw(newToken.Entity);
                    JToken selToken = rawToken.SelectToken(newToken.Path);
                    if (selToken == null) return false;
                    if (selToken.Parent != null)
                        selToken.Replace(newToken.Value);
                    else
                        rawToken = newToken.Value;
                    if (stopMonitoring) Store.Watcher.StopMonitoring();
                    Store.Set(rawToken, newToken.Entity);
                    if (stopMonitoring)
                    {
                        var res = Load(false);
                        if (res.Changes.Length > 0)
                        {
                            _logger.LogTrace("Applied Update: ");
                            foreach (var c in res.Changes)
                                _logger.LogTrace(c.diffToken?.ToString(Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                }
                else
                {
                    _logger.LogError("ApplyUpdate: Unauthorized Entity: " + newToken.Entity);
                    throw new Exception("Unauthorized Entity: " + newToken.Entity);
                }
            }
            return true;
        }
    }

    public class ConfigServerTenants
    {
        private object _lock = new object();
        public List<ConfigServerTenant> Tenants { get; set; }
        public ILogger _logger { get; private set; }

        public ConfigServerTenants(IOptions<TenantsConfigSection> tenantsConfig, IHostingEnvironment env, ILogger<ConfigServerTenant> logger)
        {
            _logger = logger;
            Tenants = tenantsConfig.Value.Tenants.Where(t=>t.Enabled).Select(t =>
            {
                ConfigServerTenant tenant = new ConfigServerTenant(t, env, logger);
                return tenant;
            }).ToList();

            _logger.LogTrace("Number of tenants: " + Tenants.Count + " tenants.");
        }

        private ConfigServerTenant GetTenant(string tenantId)
        {
            foreach (var tenant in Tenants)
            {
                if (tenant.TenantConfig.Id == tenantId)
                    return tenant;
            }
            return null;
        }



        public bool ApplyUpdate(List<LogMessage> logs, List<LogMessage> applyedlogs)
        {
            int TotalLogsApplied = 0;
            var groupedTenant = logs.GroupBy(u => u.TenantId).ToList();

            foreach (var groupT in groupedTenant)
            {
                string tenantId = groupT.Key;

                if (tenantId == null)
                {
                    _logger.LogTrace("<Null> TenantId found.");
                    continue;
                }
                ConfigServerTenant tenant = GetTenant(tenantId);
                if (tenant == null) { _logger.LogError("ApplyUpdate: Tenant: " + tenantId + " not found."); continue; }
                if (tenant.Token == null) { _logger.LogWarning("ApplyUpdate: Tenant: " + tenantId + " not loader."); continue; }
                lock (_lock)
                {
                    tenant.Store.Watcher.StopMonitoring();
                    var groupedEntities = groupT.GroupBy(u => u.Entity).ToList();
                    foreach (var group in groupedEntities)
                    {
                        string entity = group.Key;
                        if (!tenant.Store.Watcher.IsWatching(entity)) { _logger.LogError("ApplyUpdate: Unrecognized entity: " + entity + " for tanant: " + tenant); continue; }

                        var groupSorted = group.OrderBy(g => g.LogId);

                        foreach (var log in groupSorted)
                        {
                            if (string.IsNullOrEmpty(log.JsonDiff)) { continue; }
                            JToken rawToken = tenant.Store.GetRaw(entity);
                            applyedlogs.Add(log);

                            if (log.Full)
                            {
                                tenant.Store.Set(JToken.Parse(log.JsonDiff), entity);
                            }
                            else
                            {
                                JToken patchToken = JToken.Parse(log.JsonDiff);
                                var mJsonDiff = new JsonDiffPatchDotNet.JsonDiffPatch();
                                JToken result = mJsonDiff.Patch(rawToken, patchToken);
                                tenant.Store.Set(result, entity);
                            }
                        }
                    }

                    var res = tenant.Load(false);
                    TotalLogsApplied += res.Changes.Length;
                    if (res.Changes.Length > 0)
                    {
                        _logger.LogTrace("Applied Update: ");
                        foreach (var c in res.Changes)
                            _logger.LogTrace(c.diffToken?.ToString(Newtonsoft.Json.Formatting.Indented));
                    }

                }
            }
            _logger.LogInformation("Applied " + TotalLogsApplied + " logs successfully.");
            return TotalLogsApplied > 0;
        }
    }
}
