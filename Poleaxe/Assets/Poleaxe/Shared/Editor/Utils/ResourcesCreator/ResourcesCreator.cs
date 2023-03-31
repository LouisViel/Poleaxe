using UnityEditor;
using UnityEngine;
using Poleaxe.Editor.Helper;

namespace Poleaxe.Editor.Utils.ResourceCreator
{
    public class ResourceCreatorData
    {
        public string fileName = string.Empty;
        public string pluginDirectoryName = "Poleaxe";
        public string containerDirectoryName = "Runtime";
        public bool isEditor = false;
        public bool isTemp = false;
    }

    public static class ResourceCreator<t> where t : ScriptableObject
    {
        public static void Save(t current) => EditorHelper.SaveAsset(current);
        public static void Refresh(ref t current, ResourceCreatorData data)
        {
            if (current == null) {
                current = Resources.Load<t>(data.fileName);
                if (current == null) current = Create(data);
            }
        }

        public static t Create(ResourceCreatorData data)
        {
            string path = GetPath(data);
            if (string.IsNullOrEmpty(path)) path = CreatePath(data);
            string filePath = PathHelper.CombinePath(path, $"{data.fileName}.asset");
            t so = ScriptableObject.CreateInstance<t>();
            AssetDatabase.CreateAsset(so, filePath);
            AssetDatabase.SaveAssetIfDirty(so);
            return so;
        }

        public static string EnsureGetPath(ResourceCreatorData data)
        {
            string path = GetPath(data);
            if (!string.IsNullOrEmpty(path)) return path;
            return CreatePath(data);
        }

        public static string GetPath(ResourceCreatorData data)
        {
            string[] paths = PathHelper.GetDirectoryPaths(data.pluginDirectoryName);
            foreach (string path in paths) {
                if (PathHelper.TryGetDirectoryAtPath(path, data.containerDirectoryName, out string containerPath)) {
                    if (data.isTemp) {
                        if (PathHelper.TryGetDirectoryAtPath(containerPath, "Temp", out string tempPath)) {
                            if (PathHelper.TryGetDirectoryAtPath(tempPath, "Resources", out string resourcesPath)) {
                                return PathHelper.AbsolutePathToRelative(resourcesPath);
                            }
                        }
                    } else {
                        if (PathHelper.TryGetDirectoryAtPath(containerPath, "Resources", out string resourcesPath)) {
                            return PathHelper.AbsolutePathToRelative(resourcesPath);
                        }
                    }
                }
            }
            return string.Empty;
        }

        public static string CreatePath(ResourceCreatorData data)
        {
            string dir = "Resources";
            if (data.isTemp) dir = PathHelper.CombinePath("Temp", dir);
            if (data.isEditor) dir = PathHelper.CombinePath("Editor", dir);
            string path = PathHelper.CreateDirectory(dir);
            return PathHelper.AbsolutePathToRelative(path);
        }
    }
}