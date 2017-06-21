using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace yupisoft.ConfigServer.Tests
{
    [TestClass]
    public class MiscTests
    {

        private string Sc(string args)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), @"system32\\sc.exe"),
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit(3000);
            return proc.StandardOutput.ReadToEnd();
        }

        [TestMethod]
        public void CheckProcessStatusRunning()
        {
            string output = Sc("query \"Bonjour Service\"");
            string pattern = @"STATE[\s|\t]*:[\s|\t]*(\d\d?)[\s|\t]*([A-Z_]*)";
            Match m = Regex.Match(output, pattern);
            string status = "";
            if (m.Groups.Count == 3)
                status = m.Groups[2].Value;

            Assert.AreEqual<string>("RUNNING", status);
        }



    }
}
