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
        public string ACLEntityName { get; set; }

    }

    public class TenantConfigSection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool Encrypted { get; set; }
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
