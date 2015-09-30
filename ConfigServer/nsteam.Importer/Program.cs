using masterconfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nsteam.Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            masterconfigDB db = new masterconfigDB();

            string templateapp =
@"             
    {
        ""name"":""<appname>"", 
<appsettings>
    }";
            string templatesetting = @"        ""<setname>"" : ""<setvalue>""";

            string templatemain =
@"
{ 
    ""applications"": [                
    <templateapplist>
    ]
}";


            string templateacclist =
@"
{ 
    ""accounts"": [                
    <templateacclist>
    ]
}";
            string templateacc =
@"             
    {
        ""include"": "" * acc.<accname>.json"",
        ""name"": ""<accname>"",  
        ""enviroments"" : []      
<appsettings>
    }";

            var apps = db.Fetch<ref_Application>(new PetaPoco.Sql());
            var envs = db.Fetch<ref_Environment>(new PetaPoco.Sql());
            var accs = db.Fetch<conf_Account>(new PetaPoco.Sql());

            var json_main = templatemain;

            Console.WriteLine("Application Settings -----------------------------------------------");

            foreach (var app in apps)
            {
                Console.WriteLine("-->" + app.Name);
                var json_app = templateapp;
                json_app = json_app.Replace("<appname>", app.Name);
                var settings = db.Fetch<ref_ApplicationSettingType>("where ApplicationGuid = @0", app.ApplicationGuid);
                foreach(var sett in settings)
                {
                    var json_set = templatesetting;
                    json_set = json_set.Replace("<setname>", sett.KeyName);
                    json_set = json_set.Replace("<setvalue>", "");
                    Console.WriteLine("---->" + sett.KeyName);
                    json_app = json_app.Replace("<appsettings>", json_set + ",\r\n<appsettings>");
                }
                json_app = json_app.Replace(",\r\n<appsettings>", "\r\n");
                json_app = json_app.Replace("<appsettings>", "\r\n");
                json_main = json_main.Replace("<templateapplist>", json_app + ",\r\n<templateapplist>");
            }
            json_main = json_main.Replace(",\r\n<templateapplist>", "\r\n");
            json_main = json_main.Replace("<templateapplist>", "\r\n");
            Console.WriteLine(json_main);


            json_main = templatemain;
            Console.WriteLine("Enviroments Personalization Settings -----------------------------------------------");
            foreach (var acc in accs)
            {
                foreach (var env in envs)
                {
                    foreach (var app in apps)
                    {
                        Console.WriteLine("-->" + app.Name);
                        var json_app = templateapp;
                        json_app = json_app.Replace("<appname>", app.Name);
                        var settings = db.Fetch<ref_ApplicationSettingType>("where ApplicationGuid = @0", app.ApplicationGuid);
                        foreach (var sett in settings)
                        {
                            var esettings = db.Fetch<conf_ApplicationSetting>("where ApplicationGuid = @0 and EnvironmentGuid = @1 and AccountGuid = @2 and SettingTypeGuid = @3", app.ApplicationGuid, env.EnvironmentGuid, acc.AccountGuid, sett.SettingTypeGuid);
                            if (esettings.Count > 0)
                            {

                                var json_set = templatesetting;
                                json_set = json_set.Replace("<setname>", sett.KeyName);
                                json_set = json_set.Replace("<setvalue>", esettings[0].SettingValue);
                                Console.WriteLine("---->" + sett.KeyName);
                                json_app = json_app.Replace("<appsettings>", json_set + ",\r\n<appsettings>");
                            }
                        }
                        json_app = json_app.Replace(",\r\n<appsettings>", "\r\n");
                        json_app = json_app.Replace("<appsettings>", "\r\n");
                        json_main = json_main.Replace("<templateapplist>", json_app + ",\r\n<templateapplist>");
                    }
                }
            }
            Console.WriteLine(json_main);
            Console.ReadLine();
        }
    }
}
