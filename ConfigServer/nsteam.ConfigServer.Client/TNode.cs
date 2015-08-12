using System.Dynamic;

namespace nsteam.ConfigServer.Client
{    
    public class TNode
    {
        private string _path;
        private dynamic _value;

        public TNode() {
            _path = "";
            _value = null;
        }

        public TNode(string path, dynamic value)
        {
            // TODO: Complete member initialization
            this._path = path;
            this.Value = value;
        }
        public string Path { get { return _path; } set { _path = value; } }
        public dynamic Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (value == null) return; 
                if (value is string) return; 
                if (value is DynamicObject) return;

                //if (value is JObject) { _value = JsonConvert.DeserializeObject<Dictionary<string, object>>((_value as JObject).ToString()); }
                //if (value is JArray) { _value = JsonConvert.DeserializeObject<Dictionary<string, object>[]>((_value as JObject).ToString()); }

                //if (value is object[]) _value = new DynamicJsonArray(_value);
                //else _value = new DynamicJsonObject(_value);                                    
            }
        }
    }

    public class TNode<T>
    {
        private string _path;

        public TNode(string path, T value)
        {
            // TODO: Complete member initialization
            this._path = path;
            this.Value = value;
        }
        public string Path { get { return _path; } }
        public T Value { get; set; }
    }
}
