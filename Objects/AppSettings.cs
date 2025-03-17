
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FluffySettings.Objects
{
    public abstract class AppSettings
    {
        private string _settingsfilename { get; set; }
        public string Path
        {
            get
            {
                if(!Debugger.IsAttached)
                    return AppDomain.CurrentDomain.BaseDirectory + _settingsfilename;
                else
                {
                    //TODO: Fix this shitty code. If exe is placed in deeper folder, it will not work.
                    List<string> splitted = Directory.GetCurrentDirectory().Split("\\").ToList();
                    splitted = splitted.SkipLast(3).ToList();
                    return string.Join("\\", splitted) + "\\" + _settingsfilename;
                }
            }
        }
        private string Watermark
        {
            get
            {
                return @"/*


==============================================================
    FluffySettings - Open source appsettings.json manager   ||
    for .NET applications.                                  ||
    Credits: @Joleed                                        ||
    https://github.com/JoLeed/FluffySettings                ||
==============================================================


*/
";
            }
        }
        public bool AutoSave { get; private set; }
        public AppSettings(string fileName = "appsettings.json", bool autoSave = false)
        {
            AutoSave = autoSave;
            _settingsfilename = fileName;
            Load();
        }

        public void Load()
        {
            if (File.Exists(Path))
            {
                JObject? content = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path));
                if (content == null)
                    return;

                //Filling properties with appsettings json content
                var props = this.GetType().GetProperties().Where(x => content.Descendants().OfType<JProperty>().Any(e => e.Name == x.Name)).ToList();
                foreach (var prop in props)
                {
                    var value = content[prop.Name];
                    if (value != null)
                        prop.SetValue(this, value.ToObject(prop.PropertyType));
                }
            }
            else
            {
                throw new Exception($"{_settingsfilename} file was not found in directory. ({Path})");
            }
        }

        public void Save()
        {
            JObject? content = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path));
            if (content == null)
                return;

            foreach(JProperty property in content.Descendants().OfType<JProperty>().ToList())
            {
                if(this.GetType().GetProperty(property.Name) != null)
                {
                    var propInfo = this.GetType().GetProperty(property.Name);
                    if (propInfo != null)
                    {
                        var propValue = propInfo.GetValue(this);
                        property.Value = JToken.FromObject(propValue);
                    }
                }
            }

            File.WriteAllText(Path, Watermark + content.ToString());
        }
        public void Discard() => Load();
    }
}
