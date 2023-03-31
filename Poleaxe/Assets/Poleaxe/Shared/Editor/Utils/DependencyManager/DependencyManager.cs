using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Poleaxe.Editor.Utils.ResourceCreator;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Poleaxe.Editor.Utils.Dependency
{
    public static class DependencyManager
    {
        ///////////////////////////////////////////////
        ////////////////  CONSTANTS  //////////////////
        ///////////////////////////////////////////////

        #region Constants

        const string name = nameof(DependencyManager);
        static ResourceCreatorData resourceCreator = new ResourceCreatorData {
            fileName = nameof(DependencySettings),
            containerDirectoryName = "Editor",
            isEditor = true
        };

        #endregion Constants

        ///////////////////////////////////////////////
        ////////////////  VARIABLES  //////////////////
        ///////////////////////////////////////////////

        #region Variables

        private static bool DoDisplayProgressBar => toResolve.Count > 0;
        private static List<string> toResolve = new List<string>();

        private static ListRequest listRequest = null;
        private static AddAndRemoveRequest addAndRemoveRequest = null;
        private static DependencySettings dependencySettings;

        #endregion Variables

        ///////////////////////////////////////////////
        ////////////////  SETTINGS  ///////////////////
        ///////////////////////////////////////////////

        #region Settings

        private static void RefreshSettings() => ResourceCreator<DependencySettings>.Refresh(ref dependencySettings, resourceCreator);
        private static void ApplySettings() => ResourceCreator<DependencySettings>.Save(dependencySettings);

        #endregion Settings

        ///////////////////////////////////////////////
        ///////////////  INITIALIZE  //////////////////
        ///////////////////////////////////////////////

        #region Initialize

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            return; // BUG : Avoid Useless Asset Loading
            /*List<string> packages = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:PackageDependency");
            string[] paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            PackageDependency[] dependencies = paths.Select(AssetDatabase.LoadAssetAtPath<PackageDependency>).ToArray();
            foreach (PackageDependency dependency in dependencies) packages.AddRange(dependency.packages);
            AddResolution(packages.Distinct().ToArray());*/
        }

        #endregion Initialize

        ///////////////////////////////////////////////
        /////////////  ADD RESOLUTION  ////////////////
        ///////////////////////////////////////////////

        #region AddResolution

        public static void AddResolution(string[] resolution)
        {
            // Add Packages Resolutions & Start Everything
            RefreshSettings();
            if (resolution.Length <= 0) return;
            if (!dependencySettings.autoImportPackages) return;
            List<string> resolutions = GetResolutions(resolution);
            if (listRequest == null) {
                toResolve.AddRange(resolutions);
                listRequest = Client.List(false, true);
                EditorApplication.update += ListProgress;
            } else if (listRequest.IsCompleted) Resolve(resolutions);
            else toResolve.AddRange(resolutions);
            toResolve = toResolve.Distinct().ToList();
        }

        private static List<string> GetResolutions(string[] resolution)
        {
            // Get Only Resolutions not disabled in Settings
            List<string> resolutions = resolution.ToList();
            for (int i = resolutions.Count - 1; i >= 0; --i) {
                foreach (DependencyData data in dependencySettings.dependencies) {
                    if (data.name == resolutions[i] && !data.autoImport) {
                        resolutions.RemoveAt(i);
                        break;
                    }
                }
            }
            return resolutions;
        }

        #endregion AddResolution

        ///////////////////////////////////////////////
        /////////////  LIST PROGRESS  /////////////////
        ///////////////////////////////////////////////

        #region ListProgress

        private static void ListProgress()
        {
            // Included Packages Listing Progress
            if (DoDisplayProgressBar) DisplayProgressBar(0f);
            if (!listRequest.IsCompleted) return;
            if (listRequest.Status == StatusCode.Success) ListProgessSuccess();
            else Error($"{name} : Could Not Resolve Project Packages");
            EditorApplication.update -= ListProgress;
            if (DoDisplayProgressBar) DisplayProgressBar(1f);
        }

        private static void ListProgessSuccess()
        {
            // Reset & Apply IsUsed Settings
            RefreshSettings();
            foreach (DependencyData data in dependencySettings.dependencies) {
                data.isUsed = false;
                foreach (PackageInfo package in listRequest.Result) {
                    if (data.name == package.name) {
                        data.isUsed = true;
                    }
                }
            }

            // Apply Settings, then Resolve Packages
            ApplySettings();
            Resolve(toResolve);
        }

        #endregion ListProgress

        ///////////////////////////////////////////////
        ////////////////  RESOLVE  ////////////////////
        ///////////////////////////////////////////////

        #region Resolve

        private static void Resolve(List<string> packages)
        {
            // Verify if nothing Running & Refresh Settings with New Packages
            if (addAndRemoveRequest != null) return;
            Resolve_RefreshSettings(packages);

            // Remove already included Packages & Save it in Session Data
            foreach (PackageInfo package in listRequest.Result) {
                if (packages.Remove(package.name)) {
                    SessionState.SetBool(package.name, true);
                }
            }

            // If Packages, Start Adding them
            if (packages.Count <= 0) return;
            addAndRemoveRequest = Client.AddAndRemove(packages.ToArray());
            EditorApplication.update += AddAndRemoveProgress;
        }

        private static void Resolve_RefreshSettings(List<string> packages)
        {
            RefreshSettings();
            foreach (string package in packages) {
                bool doExist = false;
                foreach (DependencyData data in dependencySettings.dependencies) {
                    if (data.name == package) {
                        data.isUsed = true;
                        doExist = true;
                    }
                }
                if (!doExist) {
                    dependencySettings.dependencies.Add(new DependencyData {
                        name = package, isUsed = true, autoImport = true,
                    });
                }
            }
            ApplySettings();
        }

        #endregion Resolve

        ///////////////////////////////////////////////
        ////////  ADD AND REMOVE PROGRESS  ////////////
        ///////////////////////////////////////////////

        #region AddAndRemoveProgress

        private static void AddAndRemoveProgress()
        {
            // Packages Adding Progress
            DisplayProgressBar(0f);
            if (!addAndRemoveRequest.IsCompleted) return;
            foreach (PackageInfo package in addAndRemoveRequest.Result) SessionState.SetBool(package.name, true);
            if (addAndRemoveRequest.Status == StatusCode.Success) Client.Resolve();
            else Error($"{name} : Could Not Install Dependency Packages");
            EditorApplication.update -= AddAndRemoveProgress;
            DisplayProgressBar(1f);
        }

        #endregion AddAndRemoveProgress

        ///////////////////////////////////////////////
        /////////////////  UTILS  /////////////////////
        ///////////////////////////////////////////////

        #region Utils

        private static void DisplayProgressBar(float progress)
        {
            EditorUtility.DisplayProgressBar("Resolving Packages", $"{name} : Resolving Packages", progress);
            if (progress >= 1f) EditorUtility.ClearProgressBar();
        }

        private static void Error(string message)
        {
            Debug.LogError(message);
            addAndRemoveRequest = null;
            listRequest = null;
        }

        #endregion Utils
    }
}