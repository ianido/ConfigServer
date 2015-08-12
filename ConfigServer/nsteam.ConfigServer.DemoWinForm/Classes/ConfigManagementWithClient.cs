using nsteam.ConfigServer.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace nsteam.ConfigServer.DemoWinForm
{
    public class ConfigManagementWithClient : IConfigManagement
    {
        public ConfigService srv = new ConfigService();

        public string GetNode(string path)
        {
            dynamic obj = srv.GetNode(path);
            string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return str;
        }

        public string GetTree(string path)
        {
            dynamic obj = srv.GetTree(path);
            string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return str;
        }

        public void SetNode(string node)
        {
            try
            {
                TNode obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TNode>(node);
                if (obj.Value == null)
                {
                    
                    return;
                }
                srv.SaveNode(obj);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
