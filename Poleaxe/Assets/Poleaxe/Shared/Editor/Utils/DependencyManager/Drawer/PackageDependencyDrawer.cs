using UnityEditor;
using UnityEngine;
using Poleaxe.Editor.Helper;
using Poleaxe.Editor.Utils.Dependency;

namespace Poleaxe.Editor.Drawer
{
    [CustomEditor(typeof(PackageDependency))]
    public class PackageDependencyDrawer : UnityEditor.Editor, IHasCustomMenu
    {
        const string packagesName = "packages";

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            bool defaultEnable = GUI.enabled;
            SerializedProperty packages = serializedObject.FindProperty(packagesName);

            GUI.enabled = !((PackageDependency)target).isFreeze;
            EditorGUILayout.PropertyField(packages);

            serializedObject.ApplyModifiedProperties();
            GUI.enabled = defaultEnable;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            if (targets.Length > 1) return;
            PackageDependency dependency = (PackageDependency)target;
            menu.AddItem(EditorGUIUtility.TrTextContent("Freeze"), dependency.isFreeze, () => {
                dependency.isFreeze = !dependency.isFreeze;
                EditorHelper.SaveAsset(dependency);
            });
        }
    }
}