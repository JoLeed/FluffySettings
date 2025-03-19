# FluffySettings – the softest way to handle your appsettings.json 🐇✨

## About
Appsettings.json of your app doesn't need to be so hard to access. Say goodbye to scary in-built appsettings.json file and welcom a lightweight, fast, and ridiculously easy-to-use NuGet package for managing configuration of your .NET applications. With FluffyBunny, your settings are always within reach – clean, simple, and ready to hop into action.

⚡ Fast – Zero fluff where it matters. Just pure speed and efficiency.
🐰 Easy to Use – Load, access, and update settings with minimal code and maximum fluffiness.
🎯 Flexible – Quickly change configurations without jumping through hoops. Your bunny’s got your back.
🧁 Light as a Cupcake – No bloat. Just sweet, simple config management.

With FluffySettings you can:
- Read your appsettings.json form every part of the program
- Add properties to your settings file
- Remove properties from your settings file
- Modify properties of your settings file
- Use two modes: auto-sync (you always get synced file content), local-memory (file gets synced with its current content on any time you want)

# How to use

### Instance parameters
**fileName** (optional, Default "appsettings.json") - define custom name for your appsettings file.\
**autosave** (optional, Default false) - !DOESN'T WORK FOR NOW! saves your settings file on every change.\
**preventCreation** (optional, Default false) - prevents settings file from being created when it doesn't exist.\

### Setup
**Model:**
To gain control over your appsettings.json file your need to create a new class extending "AppSettings" which will be used as an instance of app's settings file.

First create a class:

    public class SomeSettingsFileModel : AppSettings
    {
    
    }
    
If you want to pass any settings, just add the following line to the class:

        public SettingsModel(bool autosv) : base(autoSave: autosv) { } // This part is important when you want to pass your instance parameters

**Now lets declare properties of your settings file.** Every property needs to be preceded by [AppsettingsProperty] tag:

    [AppsettingsProperty]
    public string YourPropertyName { get; set; } = "";

And that's it! Your appsettings.json file is ready!

### Full settings file class example:
    public class SettingsModel : AppSettings
    {
        public SettingsModel(bool autosv) : base(autoSave: autosv) { } // Optional
        [AppsettingsProperty]
        public string LogsLocation { get; set; } = "";
        [AppsettingsProperty]
        public string AllowedHosts { get; set; } = "";
        [AppsettingsProperty]
        public int SomeRandonINTSettings { get; set; }
        [AppsettingsProperty]
        public bool SomeRandonBoolSettings { get; set; }
        [AppsettingsProperty]
        public double SomeRandonDoubleSettings { get; set; }
        [AppsettingsProperty]
        public List<string> SomeRandonListSettings { get; set; } = new List<string> {"first", "second" };
    }
### Usage

    SettingsModel settings = new SettingsModel(false); //creating new instance of your file

When you initalize new instance of appsettings file, it automatically loads it's parameter's values.

Requesting parameter declared in your class, instantly returns value from appsettings file.\

*!!Important!! File does not automatically synchronise with the physical one on the device since **Source mirroring is still in development**. When file gets modified by another process or user, properies won't synchronise. All properties are loaded only once. When you want the current content of your file, call:*

    settings.Discard()

*This way, all changes are discarded and file is being reloaded to it's newest state.*

### When file is initialized, you can already access any property value

    settings.AllowedHosts
    > "value"

Later usage

    settings.LogsLocation = "C:\Users\Fluffy\Logs\Output" // setting parameter value
    settings.Save()
 :
 
    settings.Save()
> saves the file and applies the changes to your appsettings.json

: :
## Source mirroring functionality (under development)
Source mirroring enables your instance to be always on time with it's original state.

### It's recommended to keep the source mirroring on, but if you're motivated to do so:

In your model, add parameter to the constructor:

    ublic SettingsModel(bool autosv) : base(mirroring: autosv) { }

Then while creating an instance:

    SettingsModel settings = new SettingsModel(false); //passes false value to the constructor of the file.

### SourceChanged Event
Event is being called when source file gets modified.
This allows you to be always up-to-date with settings of your file without restarting the app or reinitializing the instance.
You can override the **SourceChanged** method to handle this event.

## Example usage

    public override void FileChanged()
    {
        // your logic here
    }
