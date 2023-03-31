using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poleaxe.Editor.Utils.Dependency
{
    [Serializable]
    internal class DependencySettings : ScriptableObject
    {
        public bool autoImportPackages = true;
        public List<DependencyData> dependencies = new List<DependencyData>();
    }

    [Serializable]
    internal class DependencyData
    {
        public string name = string.Empty;
        public bool isUsed = false;
        public bool autoImport = true;
    }
}