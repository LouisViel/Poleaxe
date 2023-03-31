#if ADDRESSABLES
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Poleaxe.Editor.Helper
{
    public static class AdressablesHelper
    {
        ///////////////////////////////////////////////
        ////////////////  VARIABLES  //////////////////
        ///////////////////////////////////////////////

        #region Variables

        const string helperName = nameof(AdressablesHelper);
        static string build_script = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
        static string settings_asset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        static string profile_name = "Default";

        private static AddressableAssetSettings settings;

        #endregion Variables

        ///////////////////////////////////////////////
        ////////////  BUILD ADDRESSABLES  /////////////
        ///////////////////////////////////////////////

        #region BuildAddressables

        public static bool BuildAddressables()
        {
            GetSettingsObject(settings_asset);
            SetProfile(profile_name);

            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) is not IDataBuilder builderScript) {
                Debug.LogError($"{helperName} : {build_script} couldn't be found or isn't a build script.");
                return false;
            }

            SetBuilder(builderScript);
            return BuildAddressableContent();
        }

        #endregion BuildAddressables

        ///////////////////////////////////////////////
        ///////  BUILD ADDRESSABLES AND PLAYER  ///////
        ///////////////////////////////////////////////

        #region BuildAddressablesAndPlayer

        public static bool BuildAddressablesAndPlayer(bool runBuild = false)
        {
            bool result = BuildAddressables();
            if (result) {
                try {
                    BuildPlayerOptions options = new BuildPlayerOptions();
                    BuildPlayerOptions playerSettings = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);
                    BuildReport buildReport = BuildPipeline.BuildPlayer(playerSettings);
                    result &= buildReport.summary.result == BuildResult.Succeeded;

                    if (runBuild) {
                        Process proc = new Process();
                        proc.StartInfo.FileName = playerSettings.locationPathName;
                        proc.Start();
                    }
                } catch (Exception) { result = false; }
            }
            return result;
        }

        #endregion BuildAddressablesAndPlayer

        ///////////////////////////////////////////////
        ///////////  GET SETTINGS OBJECT  /////////////
        ///////////////////////////////////////////////

        #region GetSettingsObject

        private static void GetSettingsObject(string settingsAsset)
        {
            settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(settingsAsset);
            if (settings == null) Debug.LogError($"{helperName} : {settingsAsset} couldn't be found or isn't a settings object.");
        }

        #endregion GetSettingsObject

        ///////////////////////////////////////////////
        ///////////////  SET PROFILE  /////////////////
        ///////////////////////////////////////////////

        #region SetProfile

        private static void SetProfile(string profile)
        {
            string profileId = settings.profileSettings.GetProfileId(profile);
            if (!string.IsNullOrEmpty(profileId)) settings.activeProfileId = profileId;
            else Debug.LogWarning($"{helperName} : Couldn't find a profile named \"{profile}\", using current profile instead.");
        }

        #endregion SetProfile

        ///////////////////////////////////////////////
        ///////////////  SET BUILDER  /////////////////
        ///////////////////////////////////////////////

        #region SetBuilder

        private static void SetBuilder(IDataBuilder builder)
        {
            int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);
            if (index > 0) settings.ActivePlayerDataBuilderIndex = index;
            else Debug.LogWarning($"{helperName} : {builder} must be added to the DataBuilders list before it can be made active. Using last run builder instead.");
        }

        #endregion SetBuilder

        ///////////////////////////////////////////////
        ////////  BUILD ADRESSABLE CONTENT  ///////////
        ///////////////////////////////////////////////

        #region BuildAddressableContent

        private static bool BuildAddressableContent()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);
            if (!success) Debug.LogError($"{helperName} : Addressables build error encountered: {result.Error}");
            return success;
        }

        #endregion BuildAddressableContent
    }
}
#endif