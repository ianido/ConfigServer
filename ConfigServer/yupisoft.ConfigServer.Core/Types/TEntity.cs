using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core
{
    public class TEntity
    {
        private string _name;
        private string _location;

        public TEntity(string name, string location)
        {
            this._name = name;
            this._location = location;
        }
        public string Name => _name;
        public string Location => _location;
    }

}
