using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Poleaxe.Editor.Helper
{
    public static class PathHelper
    {
        ///////////////////////////////////////////////
        //////////////  ALL - CONSTANTS  //////////////
        ///////////////////////////////////////////////

        const double creationDifferenceAllowed = 1.5d;

        ///////////////////////////////////////////////
        /////////////   ALL - VARIABLES   /////////////
        ///////////////////////////////////////////////

        public static char DirectorySeparator => Path.DirectorySeparatorChar;
        public static char AltDirectorySeparator => Path.AltDirectorySeparatorChar;

        public static string ApplicationDataPath => Application.dataPath;
        public static string ApplicationDataPathStrict => ApplicationDataPath.Replace("/Assets", "");

        ///////////////////////////////////////////////
        ///////////////  ALL - METHODS  ///////////////
        ///////////////////////////////////////////////

        public static string PathToName(string path) => PathToName(path, GetExtension(path));
        public static string PathToName(string path, string extension)
        {
            string[] split = path.Split(DirectorySeparator, AltDirectorySeparator);
            return PathWithoutExtension(split[^1], extension);
        }

        public static string PathWithoutExtension(string path) => PathWithoutExtension(path, path.Split('.')[^1]);
        public static string PathWithoutExtension(string path, string extension)
        {
            if (string.IsNullOrEmpty(extension)) return path;
            if (!extension.StartsWith('.')) extension = $".{extension}";
            return Regex.Replace(path, $"\\{extension}$", "");
        }

        public static string AbsolutePathToRelative(string path) => AbsolutePathToRelative(path, ApplicationDataPath);
        public static string AbsolutePathToRelative(string path, string relativePath)
        {
            Uri fileUri = new Uri(path), referenceUri = new Uri(relativePath);
            return FormatPath(referenceUri.MakeRelativeUri(fileUri).ToString());
        }

        public static string EnsurePathAsApplicationAbsolute(string path)
        {
            string ensuredPath, dirPath = FormatPath(path);
            if (dirPath.Contains(DirectorySeparator) || dirPath.Contains(AltDirectorySeparator)) {
                string fullPath = FormatPath(Path.GetFullPath(dirPath));
                string relativePath = AbsolutePathToRelative(fullPath);
                if (relativePath == fullPath) throw new ArgumentException("DirectoryPath must be an Absolute Path OR a relative Path to ApplicationDataPath");
                ensuredPath = CombinePath(ApplicationDataPath, relativePath);
            } else ensuredPath = CombinePath(ApplicationDataPath, dirPath);
            return ensuredPath;
        }

        ///////////////////////////////////////////////
        ////////////////  ALL - UTILS  ////////////////
        ///////////////////////////////////////////////

        public static bool Exists(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }

        public static bool IsJustCreated(string path)
        {
            TimeSpan difference = DateTime.Now - File.GetCreationTime(path);
            return difference.TotalSeconds < creationDifferenceAllowed;
        }

        public static string CleanUnityPath(string path)
        {
            if (path.StartsWith("Assets")) path = path.Substring(6);
            path = path.Trim(new char[] { DirectorySeparator, AltDirectorySeparator });
            return FormatPath(path.Trim());
        }

        public static string CombinePath(params string[] paths)
        {
            paths = paths.Where((string path) => !string.IsNullOrEmpty(path)).ToArray();
            return FormatPath(Path.Combine(paths));
        }

        public static string FormatPath(string path)
        {
            string format = Uri.UnescapeDataString(path);
            return format.Replace(AltDirectorySeparator, DirectorySeparator);
        }

        public static string GetExtension(string path)
        {
            return path.Split('.')[^1];
        }

        ///////////////////////////////////////////////
        //////////////////  DIRECTORY  ////////////////
        ///////////////////////////////////////////////

        public static string CreateDirectory(string directoryPath)
        {
            string path = EnsurePathAsApplicationAbsolute(directoryPath);
            if (!Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public static string[] GetDirectoryPaths(string directoryName) => GetDirectoryPaths(directoryName, ApplicationDataPath, false);
        public static string[] GetDirectoryPaths(string directoryName, string relativePath) => GetDirectoryPaths(directoryName, relativePath, false);
        public static string[] GetDirectoryPaths(string directoryName, bool ignoreCase) => GetDirectoryPaths(directoryName, ApplicationDataPath, ignoreCase);
        public static string[] GetDirectoryPaths(string directoryName, string relativePath, bool ignoreCase) => GetDirectoryPathsInternal(directoryName, FormatPath(relativePath), ignoreCase);
        private static string[] GetDirectoryPathsInternal(string search, string path, bool ignoreCase)
        {
            List<string> paths = new List<string>();
            string[] directories = Directory.GetDirectories(path);
            if (ComparePathToDirectory(path, search, ignoreCase)) paths.Add(path);
            foreach (string dir in directories) {
                string dirFormated = FormatPath(dir);
                if (ComparePathToDirectory(dirFormated, search, ignoreCase)) paths.Add(dirFormated);
                paths.AddRange(GetDirectoryPathsInternal(search, dirFormated, ignoreCase));
            }
            return paths.ToArray();
        }

        public static bool TryGetDirectoryAtPath(string searchPath, string directoryName, out string directoryPath)
        {
            string[] directories = Directory.GetDirectories(searchPath);
            foreach (string directory in directories) {
                string directoryFormated = FormatPath(directory);
                if (ComparePathToDirectory(directoryFormated, directoryName)) {
                    directoryPath = directoryFormated;
                    return true;
                }
            }
            directoryPath = string.Empty;
            return false;
        }

        public static bool ComparePathToDirectory(string path, string directoryName) => ComparePathToDirectory(path, directoryName, false);
        public static bool ComparePathToDirectory(string path, string directoryName, bool ignoreCase)
        {
            return path.EndsWith(directoryName, ignoreCase, CultureInfo.CurrentCulture);
        }

        ///////////////////////////////////////////////
        /////////////////////  FILE  //////////////////
        ///////////////////////////////////////////////

        public static string WriteAllBytes(string path, string filename, byte[] bytes) => WriteAllBytes(path, filename, bytes, true);
        public static string WriteAllBytes(string path, string filename, byte[] bytes, bool uniquePath)
        {
            string absolutePath = EnsurePathAsApplicationAbsolute(path);
            if (!Exists(absolutePath)) CreateDirectory(absolutePath);
            string filePath = CombinePath(absolutePath, filename);
            if (uniquePath) {
                string relativePath = AbsolutePathToRelative(filePath);
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(relativePath);
                filePath = EnsurePathAsApplicationAbsolute(assetPath);
            }
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }
    }
}