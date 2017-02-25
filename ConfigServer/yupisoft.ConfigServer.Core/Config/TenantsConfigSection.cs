using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public class StoreConfigSection
    {
        public string Provider { get; set; }
        public string Connection { get; set; }
        public string StartEntityName { get; set; }

    }

    public class TenantConfigSection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public StoreConfigSection Store { get; set; }

        public TenantConfigSection()
        {
            Store = new StoreConfigSection();
        }
    }

    public class TenantsConfigSection
    {

        public List<TenantConfigSection> Tenants { get; set; }

        public TenantsConfigSection()
        {
            Tenants = new List<TenantConfigSection>();
        } 

    }
}
