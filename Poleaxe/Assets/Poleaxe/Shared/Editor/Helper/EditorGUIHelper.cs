using UnityEditor;
using UnityEngine;

namespace Poleaxe.Editor.Helper
{
    public static class EditorGUIHelper
    {
        ////////////////////////////////////////////////////
        /////////////  OBJECT PROPERTY FIELD  //////////////
        ////////////////////////////////////////////////////

        #region ObjectPropertyField

        public static void ObjectPropertyField(Object @object)
        {
            if (@object == null) return;
            using (SerializedObject objectSo = new SerializedObject(@object)) {
                SerializedProperty sp = objectSo.GetIterator();
                sp.NextVisible(true);
                while (sp.NextVisible(false)) EditorGUILayout.PropertyField(sp);
                objectSo.ApplyModifiedProperties();
            }
        }

        #endregion ObjectPropertyField

        ////////////////////////////////////////////////////
        //////////  ALTERNATIVE PROPERTY FIELD  ////////////
        ////////////////////////////////////////////////////

        #region AlternativePropertyField

        public static void AlternativePropertyField(SerializedProperty serializedProperty)
        {
            if (serializedProperty == null) {
                EditorGUILayout.HelpBox("SerializedProperty is null", MessageType.Error);
                return;
            }
            SerializedProperty sp = serializedProperty.Copy();
            int startingDepth = sp.depth;
            do {
                EditorGUI.indentLevel = sp.depth; DrawAlternativePropertyField(sp);
            } while (sp.NextVisible(sp.isExpanded && !HasDefaultCustomDrawer(sp.propertyType)) && sp.depth > startingDepth);
        }

        private static void DrawAlternativePropertyField(SerializedProperty serializedProperty)
        {
            if (serializedProperty.propertyType == SerializedPropertyType.Generic) {
                serializedProperty.isExpanded = EditorGUILayout.Foldout(serializedProperty.isExpanded, serializedProperty.displayName, true);
            } else EditorGUILayout.PropertyField(serializedProperty);
        }

        private static bool HasDefaultCustomDrawer(SerializedPropertyType type)
        {
            return (
                type == SerializedPropertyType.AnimationCurve ||
                type == SerializedPropertyType.Bounds ||
                type == SerializedPropertyType.Color ||
                type == SerializedPropertyType.Gradient ||
                type == SerializedPropertyType.LayerMask ||
                type == SerializedPropertyType.ObjectReference ||
                type == SerializedPropertyType.Rect ||
                type == SerializedPropertyType.Vector2 ||
                type == SerializedPropertyType.Vector3
            );
        }

        #endregion AlternativePropertyField

        ////////////////////////////////////////////////////
        /////////////  SELECT FOLDER BUTTON  ///////////////
        ////////////////////////////////////////////////////

        #region SelectFolderButton

        public static bool SelectFolderButton(ref string path)
        {
            if (GUILayout.Button("...", GUILayout.MaxWidth(30f))) {
                path = EditorHelper.SelectFolder(path);
                GUI.FocusControl(null);
                return true;
            }
            return false;
        }

        #endregion SelectFolderButton

        ////////////////////////////////////////////////////
        //////////////  RESET VALUE BUTTON  ////////////////
        ////////////////////////////////////////////////////

        #region ResetValueButton

        public static bool ResetValueButton<t>(ref t @object, t defaultValue)
        {
            if (GUILayout.Button("↺", GUILayout.MaxWidth(30f))) {
                @object = defaultValue;
                GUI.FocusControl(null);
                return true;
            }
            return false;
        }

        #endregion ResetValueButton
    }
}