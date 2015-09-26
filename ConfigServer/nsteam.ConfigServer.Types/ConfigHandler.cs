using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    public class ConfigSettings
    {
        public ConfigHandler Configuration
        {
            get
            {
                return (ConfigHandler)ConfigurationManager.GetSection("SourcesSection");
            }
        }

        public SourcesCollection Sources
        {
            get
            {
                return this.Configuration.SourcesElement;
            }
        }

        public IEnumerable<Element> SourceElements
        {
            get
            {
                foreach (Element selement in this.Sources)
                {
                    if (selement != null)
                        yield return selement;
                }
            }
        }
    }

    public class ConfigHandler : ConfigurationSection
    {


        [ConfigurationProperty("Sources")]
        public SourcesCollection SourcesElement
        {
            get { return ((SourcesCollection)(base["Sources"])); }
            set { base["Sources"] = value; }
        }
    }

    public class Element : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }
        [ConfigurationProperty("file", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string file
        {
            get { return (string)base["file"]; }
            set { base["file"] = value; }
        }
    }
    
    [ConfigurationCollection(typeof(Element))]
    public class SourcesCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "Source";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }
        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
        }


        public override bool IsReadOnly()
        {
            return false;
        }


        protected override ConfigurationElement CreateNewElement()
        {
            return new Element();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Element)(element)).name;
        }

        public Element this[int idx]
        {
            get
            {
                return (Element)BaseGet(idx);
            }
        }
    }

}
