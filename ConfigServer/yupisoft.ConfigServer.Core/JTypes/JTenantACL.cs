using System;
using System.Collections.Generic;
using System.Text;

namespace yupisoft.ConfigServer.Core
{
    public class JTenantACL
    {
        public class JApplications
        {
            public string AppId { get; set; }
            public string Secret { get; set; }
            public string Roles { get; set; }
            public string[] RoleArray { get { return Roles == null ? new string[0]: Roles.Split(','); } }
        
        }

        public JApplications[] Apps { get; set; }
    }
}
