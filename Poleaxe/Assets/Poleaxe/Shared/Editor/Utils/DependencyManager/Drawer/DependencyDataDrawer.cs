using UnityEditor;
using UnityEngine;
using Poleaxe.Editor.Utils.Dependency;

namespace Poleaxe.Editor.Drawer
{
    [CustomPropertyDrawer(typeof(DependencyData))]
    public class DependencyDataDrawer : PropertyDrawer
    {
        const string nameName = "name";
        const string isUsedName = "isUsed";
        const string autoImportName = "autoImport";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool defaultEnable = GUI.enabled;
            SerializedProperty name = property.FindPropertyRelative(nameName);
            SerializedProperty isUsed = property.FindPropertyRelative(isUsedName);
            SerializedProperty autoImport = property.FindPropertyRelative(autoImportName);

            Rect foldoutPosition = new Rect(position);
            foldoutPosition.height = base.GetPropertyHeight(property, label);
            property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, label, true);

            if (property.isExpanded) {
                GUI.enabled = false;
                Rect namePosition = new Rect(foldoutPosition);
                namePosition.y += base.GetPropertyHeight(property, label);
                namePosition.height = EditorGUI.GetPropertyHeight(name);
                EditorGUI.PropertyField(namePosition, name);

                GUI.enabled = false;
                Rect isUsedPosition = new Rect(namePosition);
                isUsedPosition.y += EditorGUI.GetPropertyHeight(name);
                isUsedPosition.height = EditorGUI.GetPropertyHeight(isUsed);
                EditorGUI.PropertyField(isUsedPosition, isUsed);

                GUI.enabled = true;
                Rect autoImportPosition = new Rect(isUsedPosition);
                autoImportPosition.y += EditorGUI.GetPropertyHeight(isUsed);
                isUsedPosition.height = EditorGUI.GetPropertyHeight(autoImport);
                EditorGUI.PropertyField(autoImportPosition, autoImport);
            }

            property.serializedObject.ApplyModifiedProperties();
            GUI.enabled = defaultEnable;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);
            if (property.isExpanded) {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameName));
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(isUsedName));
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(autoImportName));
            }
            return height;
        }
    }
}