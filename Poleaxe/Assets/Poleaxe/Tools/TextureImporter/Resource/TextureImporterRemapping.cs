using System;
using UnityEngine;
using Poleaxe.Utils;

namespace Poleaxe.TextureImporter
{
    [CreateAssetMenu(fileName = "Texture Importer Remapping", menuName = PoleaxeConstants.AssetsToolLocation + "/Texture Importer Remapping")]
    public class TextureImporterRemapping : ScriptableObject // TODO => Faire Custom Inspector (pour le isDefault)
    {
        [HideInInspector]
        public bool isDefault = false;
        [Tooltip("The Texture Remappings")]
        public TextureRemapping[] remappings = new TextureRemapping[0];
    }

    [Serializable]
    public struct TextureRemapping // TODO => Faire Custom Inspector (pour le name)
    {
        [HideInInspector]
        public string name;
        public TextureImporterMapping fromMapping;
        public TextureImporterMapping toMapping;
        public TextureRemappingBinding[] bindings;
    }

    [Serializable]
    public struct TextureRemappingBinding // TODO => Faire Custom Inspector (pour le name)
    {
        [HideInInspector]
        public string name;
        public string fromName;
        public string toName;
        public TextureRemappingChannel repack;
        public TextureRemappingChannel unpack;
        public bool inverseValue;
    }

    public enum TextureRemappingChannel
    {
        None,
        R,
        G,
        B,
        A,
    }
}