using FluffySettings.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace FluffySettings.Objects
{
    public abstract class AppSettings
    {
        private FileSystemWatcher _sourceMirroringWatcher;
        private bool _sourceMirroring { get; set; }
        private string _settingsfilename { get; set; }
        private bool _preventAutoCreate { get; set; }
        private string _oldrawcontent { get; set; }
        private string _rawcontent { get; set; }
        private List<string> _mutatedProperties { get; set; } = new List<string>();
        private JObject? _content { get; set; }
        private JObject? _old_content { get; set; }
        public event EventHandler SourceChanged;
        public string Name { get; set; }
        protected string Path
        {
            get
            {
                if(!Debugger.IsAttached)
                    return AppDomain.CurrentDomain.BaseDirectory + _settingsfilename;
                else
                {
                    //TODO: Fix this shitty code. If exe is placed in deeper folder, it will not work.
                    List<string> splitted = System.IO.Directory.GetCurrentDirectory().Split("\\").ToList();
                    splitted = splitted.SkipLast(3).ToList();
                    return string.Join("\\", splitted) + "\\" + _settingsfilename;
                }
            }
        }
        protected string Directory
        {
            get
            {
                List<string> steps = Path.Split("\\").ToList();
                steps = steps.SkipLast(1).ToList();
                return string.Join("\\", steps);
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
        public string SourceContent
        {
            get
            {
                using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs))
                {
                    return  reader.ReadToEnd();
                }
            }
        }
        public string Content
        {
            get => _content.ToString();
        }
        public AppSettings(string fileName = "appsettings.json", bool autoSave = false, bool preventCreation = false, bool mirroring = true)
        {
            AutoSave = autoSave;
            Name = fileName;
            _sourceMirroring = mirroring;
            _settingsfilename = fileName;
            _preventAutoCreate = preventCreation;
            if(mirroring)
                RegisterSourceMirroring();
            Load();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var property in this.GetType().GetProperties().Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(AppsettingsProperty))).ToList())
            {
                sb.AppendLine($"{property.Name}: {property.GetValue(this)}");
            }
            return sb.ToString();
        }
        private void ListSourceMutations()
        {
            _mutatedProperties.Clear();
            if(_old_content == null || _content == null)
                return;

            var oldprops = _old_content.Descendants().OfType<JProperty>().ToList();
            var newprops = _content.Descendants().OfType<JProperty>().ToList();

            for(int i=0; i<oldprops.Count; i++)
            {
                if(oldprops[i].Value.ToString() != newprops[i].Value.ToString())
                {
                    _mutatedProperties.Add(oldprops[i].Name);
                }
            }
        }
        private void Load(bool keepInstanceChanges = false)
        {
            if (File.Exists(Path))
            {
                string tempOldRawContent = _oldrawcontent;
                _rawcontent = SourceContent;
                _oldrawcontent = _rawcontent;
                _old_content = _content;
                _content = JsonConvert.DeserializeObject<JObject>(_rawcontent);

                if (_content == null)
                    return;

                if (tempOldRawContent == _rawcontent)
                    return;

                if(keepInstanceChanges)
                    ListSourceMutations();

                //Filling properties with appsettings json content
                var props = this.GetType().GetProperties().Where(x => _content.Descendants().OfType<JProperty>().Any(e => e.Name == x.Name) && x.CanWrite && x.CustomAttributes.Any(x => x.AttributeType == typeof(AppsettingsProperty))).ToList();
                foreach (var prop in props)
                {
                    var sourcevalue = _content[prop.Name];
                    if (sourcevalue != null)
                    {
                        //if value is set to the new one and keepchanges is set to true, skip the property.
                        var test = prop.GetValue(this);

                        if (keepInstanceChanges && !_mutatedProperties.Contains(prop.Name))
                            continue;

                        prop.SetValue(this, sourcevalue.ToObject(prop.PropertyType));
                    }
                }

                if(SourceChanged != null)
                    SourceChanged.Invoke(this,EventArgs.Empty);

            }
            else
            {
                if(_preventAutoCreate)
                    throw new Exception($"{_settingsfilename} file was not found in directory. ({Path})");
                else
                {
                    File.WriteAllText(Path, Watermark + "{}");
                    Load();
                }
            }
        }

        private void SourceChangedHandler(object sender, FileSystemEventArgs e)
        {
            //Prevents program from invoking multiple calls on event. Can happen sometimes that Watcher invokes multiple times.
            if (e.ChangeType != WatcherChangeTypes.Changed || SourceContent == _oldrawcontent)
                return;

            Load(keepInstanceChanges: true);
        }

        //Cannot work in while loop, will overload the system. Replace this shit with something better.
        private void RegisterSourceMirroring()
        {
            _sourceMirroringWatcher = new FileSystemWatcher(Directory);
            _sourceMirroringWatcher.Filter = Name;
            _sourceMirroringWatcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite;
            _sourceMirroringWatcher.Changed += SourceChangedHandler;
            _sourceMirroringWatcher.Created += SourceChangedHandler;
            _sourceMirroringWatcher.Renamed += SourceChangedHandler;
            _sourceMirroringWatcher.Deleted += SourceChangedHandler;
            _sourceMirroringWatcher.Error += (s, e) =>
            {
                // Recover or log the error
                Console.WriteLine("FileSystemWatcher error: " + e.GetException());
            };
            _sourceMirroringWatcher.IncludeSubdirectories = true;
            _sourceMirroringWatcher.EnableRaisingEvents = true;
        }

        public void Save()
        {
            JObject? content = JsonConvert.DeserializeObject<JObject>(SourceContent);
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

            foreach(var nonexistingproperty in this.GetType().GetProperties()
                .Where(x => !content.Descendants().OfType<JProperty>().ToList().Any(e => e.Name == x.Name) &&
                x.CustomAttributes.Any(x => x.AttributeType == typeof(AppsettingsProperty))
                ))
            {
                content[nonexistingproperty.Name] = JToken.FromObject(nonexistingproperty.GetValue(this));
            }


            File.WriteAllText(Path, Watermark + content.ToString());
        }
        public void Discard() => Load();
    }
}
