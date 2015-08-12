using NLog.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using NLog;

namespace nsteam.ConfigServer.Types
{
    public class AuthRepository 
    {
        private static XmlDocument _root = null;
        private string _filename = "";
        private XmlDocument _repo
        {
            get
            {
                if (_root == null)
                {

                    if (Path.GetExtension(_filename) == ".json")
                    {
                        var jsonContent = File.ReadAllText(_filename);
                        jsonContent = Regex.Replace(jsonContent, @"("")(\w+)(""\s*)(:)(\s*"")", "$1@$2$3$4$5"); // Generate attributes based on single nodes                        
                        _root = JsonConvert.DeserializeXmlNode(jsonContent, null, true);
                    }
                    else
                    {
                        _root = new XmlDocument();
                        _root.Load(_filename);
                    }
                }
                return _root;
            }
        }
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public AuthRepository(string filename)
        {
            _filename = filename; 
        }

        private string Md5(string str)
        {
            // byte array representation of that string
            byte[] encodedPassword = new UTF8Encoding().GetBytes(str);

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

            // string representation (similar to UNIX format)
            string encoded = BitConverter.ToString(hash)
                // without dashes
               .Replace("-", string.Empty)
                // make lowercase
               .ToLower();

            // encoded contains the hash you are wanting
            return encoded;
        }

        private string GetClientSecret(string clientid)
        {
            XmlNode node = _repo.SelectSingleNode("clients/client[@id='" + clientid + "']");
            if (node == null) new ApplicationException("Client not found");
            return node.Attributes["secret"].Value;
        }

        public IdentityUser ValidateClient(string clientid, string signature)
        {
            try
            {
                string clientsecret = GetClientSecret(clientid);
                string calculated_signature = Md5(clientsecret + clientsecret);
                if (calculated_signature == signature) return new IdentityUser() { ClientId = clientid };
                return null;
            }
            catch (ApplicationException ex)
            {
                _logger.Error("ValidateClient Error.", ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.FatalException("ValidateClient Error.", ex);
                return null;
            }
        }

    }
}
