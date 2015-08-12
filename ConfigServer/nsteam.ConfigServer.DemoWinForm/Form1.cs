using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace nsteam.ConfigServer.DemoWinForm
{
    public partial class Form1 : Form
    {
        IConfigManagement _config;

        public Form1(IConfigManagement config)
        {
            _config = config;
            InitializeComponent();
        }

        private void btnTree_Click(object sender, EventArgs e)
        {
            string json = JsonHelper.FormatJson(JsonHelper.RemoveObjectInfo(_config.GetTree(txtQuery1.Text)));
            txtResults1.Text = json;
            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        }

        private void btnNode_Click(object sender, EventArgs e)
        {
            string json = JsonHelper.FormatJson(_config.GetNode(txtQuery1.Text));
            txtResults1.Text = json;
            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        }

        private void btnSave1_Click(object sender, EventArgs e)
        {
            string json = txtResults1.Text;
            _config.SetNode(json);
        }
    }
}
