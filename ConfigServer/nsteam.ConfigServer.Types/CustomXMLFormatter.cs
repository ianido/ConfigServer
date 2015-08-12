using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.ConfigServer.Types
{
    public class CustomXmlFormatter : MediaTypeFormatter
    {
        public CustomXmlFormatter()
        {
            SupportedMediaTypes.Add(
                new MediaTypeHeaderValue("application/xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
        }

        public override bool CanReadType(Type type)
        {
            if (type == (Type)null)
                throw new ArgumentNullException("type");

            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task WriteToStreamAsync(Type type, object value,
            Stream writeStream, System.Net.Http.HttpContent content,
            System.Net.TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                var json = JsonConvert.SerializeObject(value);

                string rootname = "result";
                if (json.StartsWith("["))
                {
                    json = "{\"result\" : " + json + "}";
                    rootname = "result";
                }                

                var xml = JsonConvert
                    .DeserializeXmlNode(json, rootname, true);

                xml.Save(writeStream);
            });
        }
    }
}
