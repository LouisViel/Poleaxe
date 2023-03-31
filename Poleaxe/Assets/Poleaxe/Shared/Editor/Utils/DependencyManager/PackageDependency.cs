using UnityEngine;
using Poleaxe.Utils;

namespace Poleaxe.Editor.Utils.Dependency
{
    [CreateAssetMenu(fileName = "Package Dependency", menuName = PoleaxeConstants.AssetsUtilsLocation + "/Package Dependency")]
    public class PackageDependency : ScriptableObject
    {
        [HideInInspector]
        public bool isFreeze = false;
        public string[] packages = new string[0];
    }
}