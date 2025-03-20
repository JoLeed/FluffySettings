# FluffySettings – the softest way to handle your appsettings.json 🐇✨
Appsettings.json of your app doesn't need to be so hard to access. Say goodbye to scary in-built appsettings.json file and welcom a lightweight, fast, and ridiculously easy-to-use NuGet package for managing configuration of your .NET applications. With FluffySettings, your settings are always within reach – clean, simple, and ready to hop into action.

⚡ Fast – Zero fluff where it matters. Just pure speed and efficiency.
🐰 Easy to Use – Load, access, and update settings with minimal code and maximum fluffiness.
🎯 Flexible – Quickly change configurations without jumping through hoops. Your bunny’s got your back.
🧁 Light as a Cupcake – No bloat. Just sweet, simple config management.

With FluffySettings you can:
- Read your appsettings.json form every part of the program
- Add properties to your settings file
- Remove properties from your settings file
- Modify properties of your settings file
- Create your custom json setting files, you don't especially need to use appsettings.json

# Using FluffySettings

## Requirements
**.NET** .Net 8.0 or highier

## Instance parameters
**fileName** (optional, Default "appsettings.json") - define custom name for your appsettings file.\
**autosave** (optional, Default false) - !DOESN'T WORK FOR NOW! saves your settings file on every change.\
**preventCreation** (optional, Default false) - prevents settings file from being created when it doesn't exist.\
**mirroring** (optional, Default true) - enabled file source mirroring so that your instance content is always up to date with physical file content.\

## Attributes
**AppsettingsProperty** defines that this property is a property of your appsettings file.

## Events
**SourceChanged** event which is beaing called when source file changes. Only works with [Source Mirroring](#source-mirroring) on.

## Variables
**Name** - returns string with name of your appsettings file.\
**Path** - returns full path to your appsettings file.\
**Directory** - returns directory's full path, your file is in.\
**SourceContent** - returns content directly from your physical appsettings file.\
**Content** - returns raw content that your instace operates on\

## Methods 
**Save** - saves changes to your settings file.\
**Discard** - Discards all changes and reloads the file.\


# Basic Setup
Lets show you, how to make fluffySettings work with your app. Follow theese few short steps to make it happen!

### NuGet installation:
You can directly download FluffySettings nuget from [https://www.nuget.org/](https://www.nuget.org/packages/FluffySettings) gallery.
* If you use Visual Studio 2015+ or any IDE that supports nuget gallery browser, just search for "FluffySettings".*

### After you install the NuGet, lets create a class:
To gain control over your appsettings.json file your need to create a new class extending "AppSettings" which will be used as an instance of app's settings file.

    public class SomeSettingsFileModel : AppSettings // extends "AppSettings"
    {
    
    }
    
Later if you want to pass any settings, just add the following line to the class:

        public SettingsModel(bool autosv) : base(autoSave: autosv) { } // This part is important when you want to pass your instance parameters

### Now lets declare properties of your settings file. Every property needs to be preceded by [AppsettingsProperty] tag:

    [AppsettingsProperty]
    public string YourPropertyName { get; set; } = "";

And that's it! Your appsettings.json file is ready to be controlled!

### Full settings file class example and usage:
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

In Main():

    SettingsModel settings = new SettingsModel(false); //creating new instance of your file

When you initalize new instance of appsettings file, it automatically loads it's parameter's values.

Retrieving parameter declared in your class, instantly returns value from initialized appsettings file.\

    settings.AllowedHosts
    > "value"

Later usage:

    settings.LogsLocation = "C:\Users\Fluffy\Logs\Output" // setting parameter value
    settings.Save()



# Source Mirroring
Source mirroring enables your instance to be always on time with it's original state. You dont need to reinitialize the instance or restart your app to access current file content!

## Disabling Source Mirroring
It's recommended to keep the source mirroring on, but if you're motivated to do so:

### In your model, add parameter to the constructor:

    ublic SettingsModel(bool autosv) : base(mirroring: autosv) { }

### Then while creating an instance:

    SettingsModel settings = new SettingsModel(false); //passes false value to the constructor of the file.

## SourceChanged Event for handling and detection of settings file modification
Sometimes your appsettings file [can be modified by hand](#Visual-studio-json-editor-&-SourceMirroring-conflict) or by another process. SourceChanged event allows you to handle settings modification properly!
Event is being called every time when source file gets modified.

### Example usage (Overriding the SourceChanged event)

    public override void SourceChanged()
    {
        // your logic here
    }
    
# Visual studio json editor & SourceMirroring conflict
Some text editors like "Visual studio" are not properly modyfying the text files, leaving your instance content outdated even with mirroring on.
When settings are critical for your app and you don't want to override new settings edited using this editors with the new one, always call .Discard() before modyfying property value.

# Properties protection
Some settings should be read only for yoor app. To make it so, add the **ProtectedProperty** attribute to your property.
You will be only able to modify this property from your file or other program.
This is to keep your program secure and prevent any bugs to gain access to unwanted parts.
    
        [ProtectedProperty]
        public bool AppCanDeleteSystemFile { get; set; }

