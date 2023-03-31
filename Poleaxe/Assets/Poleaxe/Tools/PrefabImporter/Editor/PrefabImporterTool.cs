using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Poleaxe.Editor.Helper;
using Poleaxe.Utils;

[Serializable]
public class PrefabImporterTool : EditorWindow
{
    ////////////////////////////////////////////
    ////////////////  CONSTANTS  ///////////////
    ////////////////////////////////////////////

    #region Constants

    const string readableToolName = "Prefab Importer";
    //static readonly string toolName = readableToolName.Replace(" ", "");

    #endregion Constants

    ////////////////////////////////////////////
    /////////////  STATIC VARIABLES  ///////////
    ////////////////////////////////////////////

    #region StaticVariables

    [MenuItem(PoleaxeConstants.ToolsLocation + "/" + readableToolName)]
    public static PrefabImporterTool OpenPrefabImporter() => GetWindow<PrefabImporterTool>(readableToolName);
    public static PrefabImporterTool Instance;

    #endregion StaticVariables

    ////////////////////////////////////////////
    //////////////  USER VARIABLES  ////////////
    ////////////////////////////////////////////

    #region UserVariables

    [SerializeField, Tooltip("Prefab to import")]
    private GameObject prefabImport;
    [SerializeField, Tooltip("Prefab to use as reference")]
    private GameObject prefabReference;
    [SerializeField, Tooltip("Folder where to save generated prefabs")]
    private string savePrefabsFolder = "Prefabs";
    [SerializeField, Tooltip("Should Renderers be used")]
    private bool useRenderer = true;
    [SerializeField, Tooltip("Should Colliders be used")]
    private bool useCollider = true;
    [SerializeField, Tooltip("Should the prefab be resized")]
    private bool useResize = false;

    #endregion UserVariables

    ////////////////////////////////////////////
    ////////////////  VARIABLES  ///////////////
    ////////////////////////////////////////////

    #region Variables

    private Vector2 scrollPosition;

    #endregion Variables

    ////////////////////////////////////////////
    //////////  SERIALIZED VARIABLES  //////////
    ////////////////////////////////////////////

    #region SerializedVariables

    SerializedObject so;
    SerializedProperty propPrefabImport;
    SerializedProperty propPrefabReference;
    SerializedProperty propSavePrefabsFolder;
    SerializedProperty propUseRenderer;
    SerializedProperty propUseCollider;
    SerializedProperty propUseResize;

    #endregion SerializedVariables

    ////////////////////////////////////////////
    ///////////////  ON ENABLE  ////////////////
    ////////////////////////////////////////////

    #region OnEnable

    void OnEnable()
    {
        Instance = this;
        so = new SerializedObject(this);
        propPrefabImport = so.FindProperty("prefabImport");
        propPrefabReference = so.FindProperty("prefabReference");
        propSavePrefabsFolder = so.FindProperty("savePrefabsFolder");
        propUseRenderer = so.FindProperty("useRenderer");
        propUseCollider = so.FindProperty("useCollider");
        propUseResize = so.FindProperty("useResize");
    }

    #endregion OnEnable

    ////////////////////////////////////////////
    ////////////////  ON GUI  //////////////////
    ////////////////////////////////////////////

    #region OnGUI

    void OnGUI()
    {
        so.Update();
        GUILayoutOption maxWidth = GUILayout.MaxWidth(500f);
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition)) {
            scrollPosition = scrollView.scrollPosition;
            using (new EditorGUILayout.VerticalScope()) {
                GUILayout.Space(2);
                
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                        EditorGUIUtility.labelWidth = 80f;
                        EditorGUILayout.PropertyField(propPrefabImport, GUILayout.MinWidth(100f), GUILayout.MaxWidth(250f));
                        EditorGUIUtility.labelWidth = 100f;
                        EditorGUILayout.PropertyField(propPrefabReference, GUILayout.MinWidth(100f), GUILayout.MaxWidth(250f));
                    }
                    GUILayout.FlexibleSpace();
                }
                
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, maxWidth)) {
                        using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.FlexibleSpace();
                            using (new EditorGUILayout.HorizontalScope()) {
                                EditorGUIUtility.labelWidth = 120f;
                                EditorGUILayout.PropertyField(propSavePrefabsFolder, GUILayout.MinWidth(100f), maxWidth);
                                EditorGUIHelper.SelectFolderButton(ref savePrefabsFolder);
                            }
                            GUILayout.FlexibleSpace();
                        }

                        using (new EditorGUILayout.HorizontalScope()) {
                            GUILayout.FlexibleSpace();
                            EditorGUIUtility.labelWidth = 85f;
                            EditorGUILayout.PropertyField(propUseRenderer);
                            GUILayout.Space(10f);
                            EditorGUIUtility.labelWidth = 75f;
                            EditorGUILayout.PropertyField(propUseCollider);
                            GUILayout.Space(10f);
                            EditorGUIUtility.labelWidth = 70f;
                            EditorGUILayout.PropertyField(propUseResize);
                            GUILayout.FlexibleSpace();
                        }
                    }
                    GUILayout.FlexibleSpace();
                }

                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    bool defaultEnable = GUI.enabled;
                    GUI.enabled = (prefabImport != null) &&
                        (useRenderer || useCollider) &&
                        (!useResize || prefabReference != null);
                    if (GUILayout.Button("Import Prefab", GUILayout.MinWidth(280f))) ImportPrefab();
                    GUI.enabled = defaultEnable;
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
    ///////////  IMPORT PREFAB  ////////////////
    ////////////////////////////////////////////

    #region ImportPrefab

    private void ImportPrefab()
    {
        GameObject go = Instantiate(prefabImport, Vector3.zero, Quaternion.identity);
        GameObject parent = new GameObject(go.name);
        Bounds bounds = GetBounds(go);

        if (useResize) {
            float scale = GetReferenceResize(bounds, prefabReference);
            go.transform.localScale = new Vector3(scale, scale, scale);
        }

        go.transform.position = -bounds.center;
        go.transform.SetParent(parent.transform);
        go.name = "Graphics";

        string folder = Regex.Replace(savePrefabsFolder, @"\s+\/\s+", "/");
        if (!folder.StartsWith("Assets")) folder = PathHelper.CombinePath("Assets", folder);
        if (!AssetDatabase.IsValidFolder(folder)) {
            Debug.LogError($"[{readableToolName}] SavePrefabFolder is not Valid");
            return;
        }

        string prefabName = $"{parent.name}.prefab";
        string path = PathHelper.CombinePath(folder, prefabName);
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(path);

        PrefabUtility.SaveAsPrefabAsset(parent, uniquePath, out bool success);
        if (success) Debug.Log($"[{readableToolName}] Prefab Imported");
        else Debug.LogError($"[{readableToolName}] Failed To Import");
        DestroyImmediate(parent);
    }

    #endregion ImportPrefab

    ////////////////////////////////////////////
    ////////  GET REFERENCE RESIZE  ////////////
    ////////////////////////////////////////////

    #region GetReferenceResize

    private float GetReferenceResize(Bounds bounds, GameObject reference)
    {
        Vector3 size = bounds.size;
        float factor = (size.x + size.y + size.z) / 3;
        GameObject referenceGo = Instantiate(reference, Vector3.zero, Quaternion.identity);
        Vector3 referenceSize = GetBounds(referenceGo).size;
        float referenceFactor = (referenceSize.x + referenceSize.y + referenceSize.z) / 3;
        DestroyImmediate(referenceGo);
        return referenceFactor / factor;
    }

    #endregion GetReferenceResize

    ////////////////////////////////////////////
    /////////////  GET BOUNDS  /////////////////
    ////////////////////////////////////////////

    #region GetBounds

    private Bounds GetBounds(GameObject go)
    {
        Bounds bounds = new Bounds();
        if (useRenderer) {
            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>()) {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        if (useCollider) {
            foreach (Collider collider in go.GetComponentsInChildren<Collider>()) {           
                bounds.Encapsulate(collider.bounds);
            }
        }
        return bounds;
    }

    #endregion GetBounds
}