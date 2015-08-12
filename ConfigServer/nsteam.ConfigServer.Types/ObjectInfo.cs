using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    
    public class ObjectInfo
    {
        public class InheritedField
        {            
            public string PropertyName { get; set; }
            public string PropertyInheritedValue { get; set; }
            public string InheritedFrom { get; set; }
            public bool Overrided { get; set; }
        }

        public class ReferencedField
        {
            public string Reference { get; set; }
            public string PropertyName { get; set; }
        }
        public List<ReferencedField> ReferencedFields { get; set; }
        public List<InheritedField> InheritedFields { get; set; }

        public ObjectInfo()
        {
            ReferencedFields = new List<ReferencedField>();
            InheritedFields = new List<InheritedField>();
        }
    }
}
