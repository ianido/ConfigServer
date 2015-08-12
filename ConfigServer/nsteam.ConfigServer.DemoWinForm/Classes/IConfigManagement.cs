using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nsteam.ConfigServer.DemoWinForm
{
    public interface IConfigManagement
    {
        string GetNode(string path);
        string GetTree(string path);
        void SetNode(string node);        
    }
}
