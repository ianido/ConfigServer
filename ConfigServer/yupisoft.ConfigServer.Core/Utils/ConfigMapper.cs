using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace yupisoft.ConfigServer.Core
{

    public class ConfigMapper : IConfigMapper
    {
        public List<TNodeMap> _nodes { get; set; }

        public ConfigMapper()
        {
            _nodes = new List<TNodeMap>();
        }

        public void Add(JToken token, string originalPath, string originalEntityName)
        {
            if (token == null) return;
            TNodeMap nm = new TNodeMap(token, originalPath, originalEntityName);
            var eIndex = _nodes.Where(e => e.Path == token.Path);
            if (eIndex.Count() == 0)
                _nodes.Add(nm);
            else
            {
                _nodes.Remove(eIndex.ToArray()[0]);
                _nodes.Add(nm);
            }
        }
        public void Clear()
        {
            _nodes.Clear();
        }
    }
}