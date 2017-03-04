using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace yupisoft.ConfigServer.Core
{
    public class TNode<T>
    {
        private string _path;
        private T _value;
        private string _entity;

        public TNode()
        {

        }

        public TNode(string path, T value, string entityName)
        {
            this._path = path;
            this._value = value;
            this._entity = entityName;
        }
        public string Path { get { return _path; } set { _path = value; } }
        public T Value { get { return _value; } set { _value = value; } }
        public string Entity { get { return _entity; } set { _entity = value; } }
    }

    public class TNode : TNode<JToken>
    {
        public TNode(string path, JToken value, string entityName) : base(path, value, entityName)
        {
        }
    }
}
