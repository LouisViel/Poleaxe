using UnityEditor;

#if UNITY_EDITOR_WIN
using Microsoft.Win32;
#elif UNITY_EDITOR_OSX
	using System.IO;
#else // LINUX
	using System.IO;
	using System.Xml;
#endif

namespace Poleaxe.Editor.Helper
{
    public static class RegistryHelper
    {
        public static string[] GetPlayerPrefsKeys()
        {
#if UNITY_EDITOR_WIN
            return ReadKeysWindows(GetPlayerPrefsPath());
#elif UNITY_EDITOR_OSX
            return ReadKeysOSX(GetPlayerPrefsPath());
#else // LINUX
            return ReadKeysLinux(GetPlayerPrefsPath());
#endif
        }

        public static string[] GetEditorPrefsKeys()
        {
#if UNITY_EDITOR_WIN
            return ReadKeysWindows(GetEditorPrefsPath());
#elif UNITY_EDITOR_OSX
            return ReadKeysOSX(GetEditorPrefsPath());
#else // LINUX
            return ReadKeysLinux(GetEditorPrefsPath());
#endif
        }

#if UNITY_EDITOR_WIN
        private static string[] ReadKeysWindows(string prefsPath)
        {
            RegistryKey registryLocation = Registry.CurrentUser.CreateSubKey(prefsPath);
            if (registryLocation == null) return new string[0];
            string[] names = registryLocation.GetValueNames();
            string[] result = new string[names.Length];
            for (int i = 0; i < names.Length; ++i) {
                string key = names[i];
                if (key.IndexOf('_') > 0) result[i] = key.Substring(0, key.LastIndexOf('_'));
                else result[i] = key;
            }
            return result;
        }

#elif UNITY_EDITOR_OSX

		private static string[] ReadKeysOSX(string plistPath)
        {
            if (!File.Exists(plistPath)) return new string[0];
            Dictionary<string, object> parsedPlist = (Dictionary<string, object>)Plist.readPlist(plistPath);
            string[] keys = new string[parsedPlist.Keys.Count];
            parsedPlist.Keys.CopyTo(keys, 0);
            return keys;
        }

#else // LINUX

		private static string[] ReadKeysLinux(string prefsPath)
        {
            if (!File.Exists(prefsPath)) return new string[0];
            XmlDocument prefsXML = new XmlDocument();
            prefsXML.Load(prefsPath);
            XmlNodeList prefsList = prefsXML.SelectNodes("/unity_prefs/pref");
            string[] keys = new string[prefsList.Count];
            for (var i = 0; i < keys.Length; i++) keys[i] = prefsList[i].Attributes["name"].Value;
            return keys;
        }
#endif

        private static string GetEditorPrefsPath()
        {
#if UNITY_EDITOR_WIN
            return "Software\\Unity Technologies\\Unity Editor 5.x";
#elif UNITY_EDITOR_OSX
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Preferences/com.unity3d.UnityEditor5.x.plist";
#else // LINUX
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/.local/share/unity3d/prefs";
#endif
        }

        private static string GetPlayerPrefsPath()
        {
#if UNITY_EDITOR_WIN
            return "Software\\Unity\\UnityEditor\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName;
#elif UNITY_EDITOR_OSX
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Preferences/unity." +
				PlayerSettings.companyName + "." + PlayerSettings.productName + ".plist";
#else // LINUX
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/.config/unity3d/" +
				PlayerSettings.companyName + "/" + PlayerSettings.productName + "/prefs";
#endif
        }
    }
}