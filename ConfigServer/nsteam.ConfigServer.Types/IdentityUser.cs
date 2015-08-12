using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    public class IdentityUser
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }        
    }
}
