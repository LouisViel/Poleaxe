using System;
using UnityEngine;

namespace Poleaxe.TextureImporter
{
    [CreateAssetMenu(fileName = "Texture Importer Mapping", menuName = "Poleaxe/Tools/Texture Importer Mapping")]
    public class TextureImporterMapping : ScriptableObject
    {
        [HideInInspector]
        public bool isDefault = false;
        [Tooltip("The Default Shader")]
        public string defaultShader = string.Empty;
        [Tooltip("The Texture Mappings")]
        public TextureMapping[] mappings = new TextureMapping[0];
    }

    [Serializable]
    public struct TextureMapping
    {
        public string name;
        public string property;
    }
}