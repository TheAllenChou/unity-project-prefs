## Per-Project Preference Utility for Unity Tools Development
by **Ming-Lun "Allen" Chou** / [AllenChou.net](http://AllenChou.net) / [@TheAllenChou](http://twitter.com/TheAllenChou) / [Patreon](https://www.patreon.com/TheAllenChou)

This is a per-project preference utility useful for developing tools for the Unity editor. It's similar to Unity's [`EditorPrefs`](https://docs.unity3d.com/ScriptReference/EditorPrefs.html), except that it stores key-value record pairs as a [scriptable object](https://docs.unity3d.com/ScriptReference/ScriptableObject.html) asset in the current project rather than modifying the machine registry like `EditorPrefs` does. This utility is for the editor only and should live in an `Editor` folder.

A minimalistic editor UI is provided for viewing and modifying the asset through Unity's inspector panel. Supported record types include: booleans, ints, floats, strings, and string sets (unique strings joined by semicolons).

![turbulent-rainbow-cubes](/img/project-prefs.png)

As an example, your tool might need a start screen that contains a check box for specifying when to automatically launch the start screen, e.g. never, on newer version, etc. This requires a way to store and access persistent data specific to this project, which can be done using this utility. Here's some sample code for this example:

```cs
using namespace LongBunnyLabs;

enum LaunchMode
{
  Never, 
  OnNewerVersion, 
};

string LaunchModeKey = "LaunchModeKey";
string LastLaunchedVersionKey = "LastLaunchedVersion";

// find int value for the "LaunchModeKey" key
LaunchMode launchMode = (LaunceMode) ProjectPrefs.GetInt(LaunchModeKey, (int) LaunchMode.OnNewerVersion);

// find int value for the "LastLaunchedVersion"
int LastLaunchedVersion = ProjectPrefs.GetInt(LastLaunchedVersionKey, -1);

bool launch = false;
switch (launchMode)
{
  case LaunchMode.Never:
    break;
    
  case LaunchMode.OnNewerVersion:
    launch = (CurrentVersion > LastLaunchedVersion);
    break;
}

if (launch)
{
  LaunchStartScreen();
  ProjectPrefs.SetInt(LastLaunchedVersionKey, CurrentVersion);
}

// ...somewhere in the start screen's scripts
LaunchMode selectedLaunchMode = (LaunchModeEnum) EditorGUILayout.EnumPopup(currentLaunchMode);
ProjectPrefs.SetInt(LaunchModeKey, (int) selectedLaunchMode);
```


## Usage

Upon any interaction with the utility, if the preference asset file doesn't exist, it will be created. The default file location is `"Assets/ProjectPrefs.asset"`. This can be changed by modifying `ProjectPrefs.InstancePath`.

In the `ProjectPrefs` class, several functions are provided for getting a value corresponding to a given key. If the preference asset file doesn't exist or if such key cannot be found, the default values passed in will be returned.

```cs
bool GetBool(string key, bool defaultValue);
int GetInt(string key, int defaultValue);
float GetFloat(string key, float defaultValue);
string GetString(string key, int defaultValue);
string[] GetSet(string key, int defaultValue);
```

There are also functions for setting the value for a given key.

```cs
void SetBool(string key, bool value);
void SetInt(string key, int value);
void SetFloat(string key, float value);
void SetString(string key, string value);
```

String sets are a bit special. A string set is a single string consisted of a set of unique strings joined by semicolons. This can be useful for representing a series of flags. There are a couple functions for string set queries and set operations.

```cs
bool SetContains(string key, string value);
void AddToSet(string key, string value);
void RemoveFromSet(string key, string value);
```

Functionally, a string set is equivalent to a collection of booleans. Which one to use is up to personal preference. The following two setups are functionally equivalent:

```cs
// using a string set
ProjectPrefs.AddToSet("MySet", "A");
ProjectPrefs.AddToSet("MySet", "D");
ProjectPrefs.AddToSet("MySet", "E");

// using booleans
ProjectPrefs.SetBool("Myset:A", true);
ProjectPrefs.SetBool("Myset:B", false);
ProjectPrefs.SetBool("Myset:C", false);
ProjectPrefs.SetBool("Myset:D", true);
ProjectPrefs.SetBool("Myset:E", true);
```
