using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    
    public class TNode
    {
        private string _path;

        public TNode(string path, dynamic value)
        {
            // TODO: Complete member initialization
            this._path = path;
            this.Value = value;
        }
        public string Path { get {return _path;}}
        public dynamic Value { get; set; }
    }
}
