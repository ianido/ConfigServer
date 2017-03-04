using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Client
{    
    public class TNode<T>
    {
        public string Path { get; set; }
        public T Value { get; set; }
        public string Entity { get; set; }
    }

    public class TNode : TNode<JToken>
    {

    }
}
