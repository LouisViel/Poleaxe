using UnityEditor;
using UnityEngine;
using Poleaxe.Editor.Helper;
using Poleaxe.Utils.Attribute;

// TODO : SOLUTION => Pour pouvoir liste qui est souscris a un Event
/*System.Action<Rect, SerializedProperty, GUIContent> test = OnGUI;
test.Target*/

namespace Poleaxe.SceneManagement.Editor.Drawer
{
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class SceneDrawer : PropertyDrawer
    {
        ////////////////////////////////////////////////////////
        /////////////////   INITIALIZATION   ///////////////////
        ////////////////////////////////////////////////////////

        /*[InitializeOnLoadMethod]
        public static void Initialize()
        {
            throw new System.NotImplementedException();
            // TODO => Ajouter "Correction" des scenes lorsqu'elles changent de nom, de path, ou de buildId
            // TODO => Subscribe to Scenes Edition (path, name, and buildId), to be able to re-set them following to their new data
            // |=> ALL OF THIS NEED TRACKING OF PROPERTIES WITH SCENE ATTRIBUTE
        }*/

        ////////////////////////////////////////////////////////
        ////////////////////   ON GUI   ////////////////////////
        ////////////////////////////////////////////////////////

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SceneAttribute sceneAttribute = (SceneAttribute)attribute;
            ValidateAttribute(sceneAttribute, property);

            if (sceneAttribute.SceneMode == SceneMode.Error) {
                EditorGUI.LabelField(position, label.text, "Use [Scene] with Strings or Integers");
                Debug.LogError($"Error with SceneAttribute on Property {property.displayName}");
                return;
            }

            SceneAsset sceneObject = null;
            if (sceneAttribute.SceneMode == SceneMode.Name) sceneObject = GetSceneByNameMode(property);
            else if (sceneAttribute.SceneMode == SceneMode.Path) sceneObject = GetSceneByPathMode(property);
            else if (sceneAttribute.SceneMode == SceneMode.BuildId) sceneObject = GetSceneByBuildIdMode(property);

            Object scene = EditorGUI.ObjectField(position, label, sceneObject, typeof(SceneAsset), true);
            SceneAsset sceneAsset = (SceneAsset)scene;

            if (sceneAttribute.SceneMode == SceneMode.Name) SetSceneByNameMode(property, sceneAsset);
            else if (sceneAttribute.SceneMode == SceneMode.Path) SetSceneByPathMode(property, sceneAsset);
            else if (sceneAttribute.SceneMode == SceneMode.BuildId) SetSceneByBuildIdMode(property, sceneAsset);
        }

        ////////////////////////////////////////////////////////
        /////////////////  GET SCENE BY MODE  //////////////////
        ////////////////////////////////////////////////////////

        private SceneAsset GetSceneByNameMode(SerializedProperty property)
        {
            if (string.IsNullOrWhiteSpace(property.stringValue)) return null;
            SceneAsset nameAsset = GetBuildSceneByName(property.stringValue);
            if (nameAsset != null) return nameAsset;
            SceneAsset pathAsset = GetBuildSceneByPath(property.stringValue);
            if (pathAsset != null) return pathAsset;
            return GetBuildSceneUsingId(property);
        }

        private SceneAsset GetSceneByPathMode(SerializedProperty property)
        {
            if (string.IsNullOrWhiteSpace(property.stringValue)) return null;
            SceneAsset pathAsset = GetBuildSceneByPath(property.stringValue);
            if (pathAsset != null) return pathAsset;
            SceneAsset nameAsset = GetBuildSceneByName(property.stringValue);
            if (nameAsset != null) return nameAsset;
            return GetBuildSceneUsingId(property);
        }

        private SceneAsset GetSceneByBuildIdMode(SerializedProperty property)
        {
            int sceneId = GetSceneBuildId(property);
            if (sceneId >= 0) return GetBuildSceneById(sceneId);
            SceneAsset nameAsset = GetBuildSceneByName(property.stringValue);
            if (nameAsset != null) return nameAsset;
            return GetBuildSceneByPath(property.stringValue);
        }

        ////////////////////////////////////////////////////////
        /////////////////  SET SCENE BY MODE  //////////////////
        ////////////////////////////////////////////////////////

        private void SetSceneByNameMode(SerializedProperty property, SceneAsset sceneAsset)
        {
            int sceneId = GetSceneBuildId(sceneAsset);
            if (sceneAsset != null && sceneId < 0) {
                Debug.LogWarning(GetNotInBuild(sceneAsset));
                property.stringValue = string.Empty;
            } else property.stringValue = sceneAsset == null ? string.Empty : sceneAsset.name;
        }

        private void SetSceneByPathMode(SerializedProperty property, SceneAsset sceneAsset)
        {
            int sceneId = GetSceneBuildId(sceneAsset);
            if (sceneAsset != null && sceneId < 0) {
                Debug.LogWarning(GetNotInBuild(sceneAsset));
                property.stringValue = string.Empty;
            } else property.stringValue = AssetDatabase.GetAssetPath(sceneAsset);
        }

        private void SetSceneByBuildIdMode(SerializedProperty property, SceneAsset sceneAsset)
        {
            int sceneId = GetSceneBuildId(sceneAsset);
            if (sceneAsset != null && sceneId < 0) {
                Debug.LogWarning(GetNotInBuild(sceneAsset));
                if (property.propertyType == SerializedPropertyType.String) property.stringValue = string.Empty;
                else if (property.propertyType == SerializedPropertyType.Integer) property.intValue = -1;
            } else if (property.propertyType == SerializedPropertyType.String) property.stringValue = sceneId.ToString();
            else if (property.propertyType == SerializedPropertyType.Integer) property.intValue = sceneId;
        }

        ////////////////////////////////////////////////////////
        //////////////////  GET BUILD SCENE  ///////////////////
        ////////////////////////////////////////////////////////

        private SceneAsset GetBuildSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return null;
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes) {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                if (sceneAsset != null && sceneAsset.name == sceneName) return sceneAsset;
            }
            return null;
        }

        private SceneAsset GetBuildSceneByPath(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath)) return null;
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes) {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                if (sceneAsset != null && buildScene.path == scenePath) return sceneAsset;
            }
            return null;
        }

        private SceneAsset GetBuildSceneById(int sceneId)
        {
            if (sceneId < 0 || sceneId > EditorBuildSettings.scenes.Length) return null;
            EditorBuildSettingsScene scene = EditorBuildSettings.scenes[sceneId];
            if (scene != null) return AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            return null;
        }

        private SceneAsset GetBuildSceneUsingId(SerializedProperty property)
        {
            int sceneId = GetSceneBuildId(property);
            if (sceneId < 0) return null;
            return GetBuildSceneById(sceneId);
        }

        ////////////////////////////////////////////////////////
        ////////////////////  SCENE UTILS  /////////////////////
        ////////////////////////////////////////////////////////

        private int GetSceneBuildId(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String) {
                if (int.TryParse(property.stringValue, out int id)) return id;
            } else if (property.propertyType == SerializedPropertyType.Integer) {
                return property.intValue;
            }
            return -1;
        }

        private int GetSceneBuildId(SceneAsset sceneAsset)
        {
            if (sceneAsset == null) return -1;
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i) {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                string sceneName = PathHelper.PathToName(scene.path);
                if (sceneAsset.name == sceneName) return i;
            }
            return -1;
        }

        ////////////////////////////////////////////////////////
        /////////////////   ATTRIBUTE UTILS   //////////////////
        ////////////////////////////////////////////////////////

        private void ValidateAttribute(SceneAttribute attribute, SerializedProperty property)
        {
            if (attribute.SceneMode == SceneMode.Default) {
                if (property.propertyType == SerializedPropertyType.String) attribute.SetSceneMode(SceneMode.Name);
                else if (property.propertyType == SerializedPropertyType.Integer) attribute.SetSceneMode(SceneMode.BuildId);
                else attribute.SetSceneMode(SceneMode.Error);

            } else if (attribute.SceneMode == SceneMode.Name || attribute.SceneMode == SceneMode.Path) {
                if (property.propertyType != SerializedPropertyType.String) {
                    attribute.SetSceneMode(SceneMode.Error);
                }

            } else if (attribute.SceneMode == SceneMode.BuildId) {
                if (property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String) {
                    attribute.SetSceneMode(SceneMode.Error);
                }
            }
        }

        ////////////////////////////////////////////////////////
        //////////////////////   UTILS   ///////////////////////
        ////////////////////////////////////////////////////////
        
        private string GetNotInBuild(SceneAsset sceneAsset)
        {
            const string errorMessage = "SceneAsset \"{SCENE}\" isn't added to Build Settings";
            return errorMessage.Replace("{SCENE}", sceneAsset == null ? "Unkown" : sceneAsset.name);
        }
    }
}