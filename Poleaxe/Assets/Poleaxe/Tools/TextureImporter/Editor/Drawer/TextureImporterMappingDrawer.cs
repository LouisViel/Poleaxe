using UnityEditor;
using UnityEngine;

namespace Poleaxe.TextureImporter.Editor.Drawer
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TextureImporterMapping))]
    public class TextureImporterMappingEditor : UnityEditor.Editor, IHasCustomMenu
    {
        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Texture Importer")) {
                TextureImporterTool tool = EditorWindow.GetWindow<TextureImporterTool>();
                TextureImporterMapping mapping = (TextureImporterMapping)targets[0];
                Selection.activeObject = mapping;
                tool.textureMapping = mapping;
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            if (targets.Length > 1) return;
            TextureImporterMapping mapping = (TextureImporterMapping)target;
            GUIContent guiContent = new GUIContent($"Default Tag");
            menu.AddItem(guiContent, mapping.isDefault, () => {
                mapping.isDefault = !mapping.isDefault;
                EditorUtility.SetDirty(mapping);
                AssetDatabase.SaveAssetIfDirty(mapping);
            });
        }
    }
}