using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core
{
    public class TService
    {
        private string Name { get; set; }
        private string Address { get; set; }
        private int Port { get; set; }
        private string[] Tags { get; set; }
        private TServiceCheck[] Checks { get; set; }

        public TService()
        {

        }

    }
}
