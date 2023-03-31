using UnityEditor;
using UnityEngine;
using Poleaxe.Editor.Helper;
using Poleaxe.Editor.Utils.Dependency;

namespace Poleaxe.Editor.Drawer
{
    [CustomEditor(typeof(DependencySettings))]
    public class DependencySettingsEditor : UnityEditor.Editor, IHasCustomMenu
    {
        const string autoImportPackagesName = "autoImportPackages";
        const string dependenciesName = "dependencies";

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            bool defaultEnable = GUI.enabled;
            SerializedProperty autoImportPackages = serializedObject.FindProperty(autoImportPackagesName);
            SerializedProperty dependencies = serializedObject.FindProperty(dependenciesName);

            GUI.enabled = true;
            EditorGUILayout.PropertyField(autoImportPackages);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(dependencies);

            serializedObject.ApplyModifiedProperties();
            GUI.enabled = defaultEnable;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            if (targets.Length > 1) return;
            menu.AddItem(EditorGUIUtility.TrTextContent("Remove Unused"), false, () => {
                DependencySettings settings = (DependencySettings)target;
                settings.dependencies.RemoveAll((DependencyData data) => !data.isUsed);
                EditorHelper.SaveAsset(settings);
            });
        }
    }
}