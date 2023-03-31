using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Poleaxe.Editor.Helper;
using Poleaxe.Editor.Utils.AssetProcessor;
using Poleaxe.Utils;
using Poleaxe.Utils.Event;

[Serializable]
public class SceneManagerTool : EditorWindow, IHasCustomMenu
{
    ////////////////////////////////////////////
    ///////////////  STRUCTURES  ///////////////
    ////////////////////////////////////////////

    #region Structures

    private struct SceneData
    {
        public string name;
        public string path;
    }

    private enum SceneAction
    {
        Open,
        OpenAdditive,
        Import,
        Delete
    }

    #endregion Structures

    ////////////////////////////////////////////
    ///////////////  CONSTANTS  ////////////////
    ////////////////////////////////////////////

    #region Constants

    const string readableToolName = "Scene Manager";
    const string importingScene = "Assets/ImportingScene.unity";

    static readonly string toolName = readableToolName.Replace(" ", "");
    static string PrefsKey => $"{EditorHelper.UniqueIdentifier}.{toolName}";

    #endregion Constants

    ////////////////////////////////////////////
    ////////////  STATIC VARIABLES  ////////////
    ////////////////////////////////////////////

    #region StaticVariables
    
    [MenuItem(PoleaxeConstants.ToolsLocation + "/" + readableToolName)]
    public static SceneManagerTool OpenSceneManager() => GetWindow<SceneManagerTool>(readableToolName);
    public static SceneManagerTool Instance;

    #endregion StaticVariables

    ////////////////////////////////////////////
    //////////////// VARIABLES  ////////////////
    ////////////////////////////////////////////

    #region Variables

    SerializedObject so = null;
    private SceneData[] scenes = new SceneData[0];
    private Vector2 scrollPosition = Vector2.zero;
    bool[] isExpanded = new bool[Enum.GetValues(typeof(SceneAction)).Length];

    #endregion Variables

    ////////////////////////////////////////////
    //////////////// ON ENABLE /////////////////
    ////////////////////////////////////////////

    #region OnEnable

    void OnEnable()
    {
        Instance = this;
        so = new SerializedObject(this);
        AssetProcessor.RegisterToAll(OnProcessAsset);
        CollectProjectScenes();
    }

    #endregion OnEnable

    ////////////////////////////////////////////
    /////////////// ON DISABLE /////////////////
    ////////////////////////////////////////////

    #region OnDisable

    void OnDisable()
    {
        AssetProcessor.RemoveFromAll(OnProcessAsset);
    }

    #endregion OnDisable

    ////////////////////////////////////////////
    ////////////////  ON GUI  //////////////////
    ////////////////////////////////////////////

    #region OnGUI

    void OnGUI()
    {
        so.Update();
        GUIStyle style = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleCenter, fontSize = 22 };
        GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(25f) };
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition)) {
            scrollPosition = scrollView.scrollPosition;

            // OPEN SCENE
            using (new EditorGUILayout.VerticalScope()) {
                GUILayout.Space(2);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Open Scenes", style, options);
                    GUILayout.Space(4f);
                    string content = isExpanded[(int)SceneAction.Open] ? "Hide Scenes" : "Show Scenes";
                    isExpanded[(int)SceneAction.Open] = EditorGUILayout.Foldout(isExpanded[(int)SceneAction.Open], content, true);
                    if (isExpanded[(int)SceneAction.Open]) {
                        GUILayout.Space(2f);
                        for (int i = 0; i < scenes.Length; ++i) {
                            using (new EditorGUILayout.HorizontalScope()) {
                                string name = $"Open {scenes[i].name}";
                                if (GUILayout.Button(name)) OpenScene(scenes[i]);
                                if (GUILayout.Button("-", GUILayout.MaxWidth(30f))) HideScene(scenes[i]);
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            // OPEN SCENE ADDITIVE
            using (new EditorGUILayout.VerticalScope()) {
                GUILayout.Space(2);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Open Scenes Additive", style, options);
                    GUILayout.Space(4f);
                    string content = isExpanded[(int)SceneAction.OpenAdditive] ? "Hide Scenes" : "Show Scenes";
                    isExpanded[(int)SceneAction.OpenAdditive] = EditorGUILayout.Foldout(isExpanded[(int)SceneAction.OpenAdditive], content, true);
                    if (isExpanded[(int)SceneAction.OpenAdditive]) {
                        GUILayout.Space(2f);
                        for (int i = 0; i < scenes.Length; ++i) {
                            using (new EditorGUILayout.HorizontalScope()) {
                                string name = $"Open Additive {scenes[i].name}";
                                if (GUILayout.Button(name)) OpenSceneAdditive(scenes[i]);
                                if (GUILayout.Button("-", GUILayout.MaxWidth(30f))) HideScene(scenes[i]);
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            // LOAD SCENE
            using (new EditorGUILayout.VerticalScope()) {
                GUILayout.Space(2);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Import Scenes", style, options);
                    GUILayout.Space(4f);
                    string content = isExpanded[(int)SceneAction.Import] ? "Hide Scenes" : "Show Scenes";
                    isExpanded[(int)SceneAction.Import] = EditorGUILayout.Foldout(isExpanded[(int)SceneAction.Import], content, true);
                    if (isExpanded[(int)SceneAction.Import]) {
                        GUILayout.Space(2f);
                        for (int i = 0; i < scenes.Length; ++i) {
                            using (new EditorGUILayout.HorizontalScope()) {
                                string name = $"Import {scenes[i].name}";
                                if (GUILayout.Button(name)) ImportScene(scenes[i]);
                                if (GUILayout.Button("-", GUILayout.MaxWidth(30f))) HideScene(scenes[i]);
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            // DELETE SCENE
            using (new EditorGUILayout.VerticalScope()) {
                GUILayout.Space(2);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Delete Scenes", style, options);
                    GUILayout.Space(4f);
                    string content = isExpanded[(int)SceneAction.Delete] ? "Hide Scenes" : "Show Scenes";
                    isExpanded[(int)SceneAction.Delete] = EditorGUILayout.Foldout(isExpanded[(int)SceneAction.Delete], content, true);
                    if (isExpanded[(int)SceneAction.Delete]) {
                        GUILayout.Space(2f);
                        for (int i = 0; i < scenes.Length; ++i) {
                            using (new EditorGUILayout.HorizontalScope()) {
                                string name = $"Delete {scenes[i].name}";
                                if (GUILayout.Button(name)) DeleteScene(scenes[i]);
                                if (GUILayout.Button("-", GUILayout.MaxWidth(30f))) HideScene(scenes[i]);
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }
        }
        so.ApplyModifiedProperties();

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
            GUI.FocusControl(null);
            Repaint();
        }
    }

    #endregion OnGUI

    ////////////////////////////////////////////
    //////////  ON PROCESS ASSET  //////////////
    ////////////////////////////////////////////

    #region OnProcessAsset

    private void OnProcessAsset(object _, EventData<AssetProcessing> data)
    {
        string current = data.Data.current, previous = data.Data.previous;
        if (current.EndsWith(PoleaxeConstants.SceneExtension) || previous.EndsWith(PoleaxeConstants.SceneExtension)) {
            CollectProjectScenes();
            Repaint();
        }
    }

    #endregion

    ////////////////////////////////////////////
    ////////////// GET SCENES //////////////////
    ////////////////////////////////////////////

    #region CollectProjectScenes

    private void CollectProjectScenes()
    {
        string[] guids = AssetDatabase.FindAssets("t:scene");
        IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
        scenes = paths.Select((string path) => {
            string split = path.Split('\\', '/')[^1];
            string name = Regex.Replace(split, "\\.unity$", "");
            return new SceneData { name = name, path = path };
        }).Where((SceneData s) => s.path.StartsWith("Assets"))
        .Where((SceneData s) => !EditorPrefs.GetBool(SceneKey(s), false)).ToArray();
    }

    #endregion CollectProjectScenes

    ////////////////////////////////////////////
    ////////////// OPEN SCENE //////////////////
    ////////////////////////////////////////////

    #region OpenScene

    private void OpenScene(SceneData scene)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
    }
    
    #endregion OpenScene

    ////////////////////////////////////////////
    ////////// OPEN SCENE ADDITIVE /////////////
    ////////////////////////////////////////////

    #region OpenSceneAdditive

    private void OpenSceneAdditive(SceneData scene)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
    }

    #endregion OpenSceneAdditive

    ////////////////////////////////////////////
    ///////////// IMPORT SCENE /////////////////
    ////////////////////////////////////////////

    #region ImportScene

    private void ImportScene(SceneData scene)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!AssetDatabase.CopyAsset(scene.path, importingScene)) {
            Debug.LogError($"[{readableToolName}] Une erreur est survenue en chargant la scene");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        Scene openScene = EditorSceneManager.OpenScene(importingScene, OpenSceneMode.Additive);

        GameObject root = new GameObject($"{scene.name} - Import");
        SceneManager.MoveGameObjectToScene(root, activeScene);

        foreach (GameObject go in openScene.GetRootGameObjects()) {
            SceneManager.MoveGameObjectToScene(go, activeScene);
            go.transform.parent = root.transform;
        }

        if (!EditorSceneManager.CloseScene(openScene, true))
            Debug.LogError($"[{readableToolName}] Une erreur est survenue en fermant la scene");
        if (!AssetDatabase.DeleteAsset(importingScene))
            Debug.LogError($"[{readableToolName}] Une erreur est survenue en supprimant la scene");
        Undo.RegisterCreatedObjectUndo(root, $"[{readableToolName}] Import Scene");
    }

    #endregion ImportScene

    ////////////////////////////////////////////
    ////////////  DELETE SCENE  ////////////////
    ////////////////////////////////////////////

    #region DeleteScene

    private void DeleteScene(SceneData scene)
    {
        if (EditorUtility.DisplayDialog("Delete Scene", $"Are you sure you want to delete the scene \"{scene.name}\"", "Continue", "Cancel")) {
            if (!AssetDatabase.DeleteAsset(scene.path)) Debug.LogError($"[{readableToolName}] Une erreur est survenue en supprimant la scene");
        }
    }

    #endregion DeleteScene

    ////////////////////////////////////////////
    ////////  SET SCENE VISIBILITY  ////////////
    ////////////////////////////////////////////

    #region SetSceneVisibility

    private void HideScene(SceneData sceneData)
    {
        string key = SceneKey(sceneData);
        EditorPrefs.SetBool(key, true);
        CollectProjectScenes();
        Repaint();
    }

    private void ShowAllScenes()
    {
        string identifier = PrefsKey;
        string[] editorPrefsKeys = RegistryHelper.GetEditorPrefsKeys();
        foreach (string key in editorPrefsKeys) {
            if (!key.StartsWith(identifier)) continue;
            EditorPrefs.DeleteKey(key);
        }
        CollectProjectScenes();
        Repaint();
    }

    #endregion SetSceneVisibility

    ////////////////////////////////////////////
    /////////////  SCENE KEY  //////////////////
    ////////////////////////////////////////////

    #region SceneKey

    private string SceneKey(SceneData scene)
    {
        string path = PathHelper.FormatPath(scene.path);
        string separator = PathHelper.DirectorySeparator.ToString();
        return $"{PrefsKey}.{path.Replace(separator, "").Replace("_", "")}";
    }

    #endregion SceneKey

    ////////////////////////////////////////////
    /////////  ADD ITEMS TO MENU  //////////////
    ////////////////////////////////////////////

    #region AddItemsToMenu

    public void AddItemsToMenu(GenericMenu menu)
    {
        GUIContent content = EditorGUIUtility.TrTextContent("Show All Scenes");
        menu.AddItem(content, false, ShowAllScenes);
    }

    #endregion AddItemsToMenu
}