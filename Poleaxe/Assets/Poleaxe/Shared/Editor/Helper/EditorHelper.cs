using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Poleaxe.Editor.Helper
{
    public static class EditorHelper
    {
        ////////////////////////////////////////////////////
        ///////////////////  CONSTANTS  ////////////////////
        ////////////////////////////////////////////////////

        public static string UniqueIdentifier => PlayerSettings.productGUID.ToString();

        ////////////////////////////////////////////////////
        /////////////////////  UTILS  //////////////////////
        ////////////////////////////////////////////////////

        public static string[] GetUserLayers()
        {
            IEnumerable<string> layers = Enumerable.Range(0, 32).Select((int i) => LayerMask.LayerToName(i));
            return layers.Where((string l) => !string.IsNullOrEmpty(l)).ToArray();
        }

        public static string SelectFolder(string current)
        {
            string absolutePath = PathHelper.EnsurePathAsApplicationAbsolute(current);
            string path = EditorUtility.OpenFolderPanel("Select Folder", absolutePath, string.Empty);
            if (string.IsNullOrEmpty(path)) return current;
            string newPath = PathHelper.AbsolutePathToRelative(path);
            return PathHelper.CleanUnityPath(newPath);
        }

        public static void SaveAsset(UnityEngine.Object @object)
        {
            EditorUtility.SetDirty(@object);
            AssetDatabase.SaveAssetIfDirty(@object);
        }

        ////////////////////////////////////////////////////
        ////////////////  UNITY VALIDATION  ////////////////
        ////////////////////////////////////////////////////

        public static bool IsBuildScene(string str)
        {
            return EditorBuildSettings.scenes.Any((EditorBuildSettingsScene s) => PathHelper.PathToName(s.path, ".unity") == str || s.path == str);
        }

        public static bool IsTag(string str)
        {
            return InternalEditorUtility.tags.Contains(str);
        }

        public static bool IsPrefab(GameObject go)
        {
            PrefabAssetType type = PrefabUtility.GetPrefabAssetType(go);
            return (type == PrefabAssetType.Model || type == PrefabAssetType.Regular || type == PrefabAssetType.Variant);
        }

        ////////////////////////////////////////////////////
        ////////////////  ARRAY VALIDATION  ////////////////
        ////////////////////////////////////////////////////

        public static bool IsValid<t>(t[] array)
        {
            return WithoutNull(array).Length == array.Length;
        }

        public static bool IsValid<t>(List<t> list)
        {
            return WithoutNull(list).Count == list.Count;
        }

        ////////////////////////////////////////////////////
        ///////////////////  MODIFIERS  ////////////////////
        ////////////////////////////////////////////////////

        public static t[] WithoutNull<t>(t[] array)
        {
            return array.Where((t o) => !IsNull(o)).ToArray();
        }

        public static List<t> WithoutNull<t>(List<t> list)
        {
            return list.Where((t o) => !IsNull(o)).ToList();
        }

        ////////////////////////////////////////////////////
        ///////////////////  INTERNAL  /////////////////////
        ////////////////////////////////////////////////////

        private static Type strType = typeof(string);
        private static bool IsNull<t>(t o)
        {
            return (typeof(t) == strType) ? string.IsNullOrEmpty(o as string) : o == null;
        }
    }
}