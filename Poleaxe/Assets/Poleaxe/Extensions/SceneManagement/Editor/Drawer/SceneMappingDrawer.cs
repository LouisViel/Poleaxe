using UnityEditor;
using UnityEngine;
using Poleaxe.Utils.Resource;

namespace Poleaxe.SceneManagement.Editor.Drawer
{
    [CustomEditor(typeof(SceneMapping))]
    public class SceneMappingDrawer : UnityEditor.Editor
    {
        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            bool defaultEnable = GUI.enabled;
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = defaultEnable;
        }
    }
}