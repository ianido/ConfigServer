using System.Dynamic;

namespace yupisoft.ConfigServer.Core
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
            this._path = path;
            this.Value = value;
        }
        public string Path {
            get {
                return _path;
            }
            set {
                _path = value;
            }
        }
        public object Value
        {
            get{
                return _value;
            }
            set{
                _value = value;                                                   
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
