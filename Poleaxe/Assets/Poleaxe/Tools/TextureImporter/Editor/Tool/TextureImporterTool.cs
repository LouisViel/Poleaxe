using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Poleaxe.Editor.Helper;

namespace Poleaxe.TextureImporter.Editor
{
    [Serializable]
    public class TextureImporterTool : EditorWindow
    {
        ////////////////////////////////////////////
        ///////////////  STRUCTURES  ///////////////
        ////////////////////////////////////////////

        #region Structures

        [Serializable]
        private struct MaterialData
        {
            public string name;
            public string path;
        }

        [Serializable]
        private struct TextureData
        {
            public string name;
            public string path;
        }

        #endregion Structures

        ////////////////////////////////////////////
        ///////////// STATIC VARIABLES /////////////
        ////////////////////////////////////////////

        #region StaticVariables

        [MenuItem("Tools/Poleaxe/Texture Importer")]
        public static TextureImporterTool OpenTextureImporter() => GetWindow<TextureImporterTool>("Texture Importer");
        public static TextureImporterTool Instance;
        const string toolName = "TextureImporter";
        const string standardShader = "Standard";

        #endregion StaticVariables

        ////////////////////////////////////////////
        //////// VARIABLES POUR L'UTILISATEUR //////
        ////////////////////////////////////////////

        #region UserVariables

        [SerializeField, Tooltip("The Texture Folder Path")]
        private string textureFolderPath = string.Empty;
        [SerializeField, Tooltip("The Material Folder Path")]
        private string materialFolderPath = string.Empty;
        [SerializeField, Tooltip("The Texture Name Separator")]
        private string textureNameSeparator = "_";
        [SerializeField, Tooltip("The Material Name Separator")]
        private string materialNameSeparator = "_";
        [SerializeField, Tooltip("Do Create Material if not found")]
        private bool createMaterial = true;

        [SerializeField, Tooltip("The Texture Mapping")]
        internal TextureImporterMapping textureMapping;

        #endregion UserVariables

        ////////////////////////////////////////////
        ///////// USER VARIABLES SERIALISED ////////
        ////////////////////////////////////////////

        #region UserVariablesSerialized

        SerializedObject so;
        SerializedProperty propTextureFolderPath;
        SerializedProperty propMaterialFolderPath;
        SerializedProperty propTextureNameSeparator;
        SerializedProperty propMaterialNameSeparator;
        SerializedProperty propCreateMaterial;
        SerializedProperty propTextureMapping;

        #endregion UserVariablesSerialized

        ////////////////////////////////////////////
        //////////////// VARIABLES /////////////////
        ////////////////////////////////////////////

        #region Variables

        private Vector2 scrollPosition = Vector2.zero;
        private List<MaterialData> materialDatas = new List<MaterialData>();
        private string materialPath = string.Empty;

        #endregion Variables

        ////////////////////////////////////////////
        //////////////// ON ENABLE /////////////////
        ////////////////////////////////////////////

        #region OnEnable

        void OnEnable()
        {
            Instance = this;
            so = new SerializedObject(this);
            propTextureFolderPath = so.FindProperty("textureFolderPath");
            propMaterialFolderPath = so.FindProperty("materialFolderPath");
            propTextureNameSeparator = so.FindProperty("textureNameSeparator");
            propMaterialNameSeparator = so.FindProperty("materialNameSeparator");
            propCreateMaterial = so.FindProperty("createMaterial");
            propTextureMapping = so.FindProperty("textureMapping");
        }

        #endregion OnEnable

        ////////////////////////////////////////////
        //////////////// ON DESTROY ////////////////
        ////////////////////////////////////////////

        #region OnDestroy

        void OnDestroy()
        {
            AssetDatabase.SaveAssets();
        }

        #endregion OnDestroy

        ////////////////////////////////////////////
        ///////////////// ON GUI ///////////////////
        ////////////////////////////////////////////

        #region OnGui

        void OnGUI()
        {
            so.Update();
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition)) {
                scrollPosition = scrollView.scrollPosition;
                using (new EditorGUILayout.VerticalScope()) {
                    GUILayout.Space(5f);

                    using (new EditorGUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUIUtility.labelWidth = 122.5f;
                            using (new EditorGUILayout.HorizontalScope()) {
                                EditorGUILayout.PropertyField(propTextureFolderPath, GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
                                if (GUILayout.Button("...", GUILayout.MaxWidth(30f))) {
                                    textureFolderPath = EditorHelper.SelectFolder(textureFolderPath);
                                    GUI.FocusControl(null);
                                }
                            }
                            EditorGUIUtility.labelWidth = 122.5f;
                            using (new EditorGUILayout.HorizontalScope()) {
                                EditorGUILayout.PropertyField(propMaterialFolderPath, GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
                                if (GUILayout.Button("...", GUILayout.MaxWidth(30f))) {
                                    materialFolderPath = EditorHelper.SelectFolder(materialFolderPath);
                                    GUI.FocusControl(null);
                                }
                                
                            }
                            EditorGUIUtility.labelWidth = 147.5f;
                            using (new EditorGUILayout.HorizontalScope()) {
                                EditorGUILayout.PropertyField(propTextureNameSeparator, GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
                                if (GUILayout.Button("↺", GUILayout.MaxWidth(30f))) {
                                    textureNameSeparator = "_";
                                    GUI.FocusControl(null);
                                }
                            }
                            EditorGUIUtility.labelWidth = 147.5f;
                            using (new EditorGUILayout.HorizontalScope()) {
                                EditorGUILayout.PropertyField(propMaterialNameSeparator, GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
                                if (GUILayout.Button("↺", GUILayout.MaxWidth(30f))) {
                                    materialNameSeparator = "_";
                                    GUI.FocusControl(null);
                                }
                            }
                            EditorGUIUtility.labelWidth = 97.5f;
                            EditorGUILayout.PropertyField(propCreateMaterial, GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
                        }
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.Space(2f);

                    using (new EditorGUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                            EditorGUIUtility.labelWidth = 100f;
                            EditorGUILayout.PropertyField(propTextureMapping, GUILayout.MinWidth(100f), GUILayout.MaxWidth(350f));
                            if (textureMapping != null) {
                                if (textureMapping.isDefault) GUI.enabled = false;
                                EditorGUIHelper.ObjectPropertyField(textureMapping);
                                if (textureMapping.isDefault) GUI.enabled = true;
                            }
                        }
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.Space(2f);

                    using (new EditorGUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        bool currentEnable = GUI.enabled;
                        GUI.enabled = textureMapping != null;
                        if (GUILayout.Button("Import Textures", GUILayout.MinWidth(280f))) ImportTextures();
                        GUI.enabled = currentEnable;
                        GUILayout.FlexibleSpace();
                    }
                }
            }

            Event current = Event.current;
            if (so.ApplyModifiedProperties()) Repaint();
            if (current.type == EventType.MouseDown && current.button == 0) {
                GUI.FocusControl(null);
                Repaint();
            }
        }

        #endregion OnGui

        ////////////////////////////////////////////
        ///////////// IMPORT TEXTURES //////////////
        ////////////////////////////////////////////

        #region ImportTextures

        private void ImportTextures()
        {
            try {
                if (textureMapping == null) return;
                if (DisplayCancelableProgressBar(0f, "Loading Textures")) return;

                string absoluteTexturePath = PathHelper.EnsurePathAsApplicationAbsolute(textureFolderPath);
                string unityTexturePath = PathHelper.AbsolutePathToRelative(absoluteTexturePath);
                string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new string[] { unityTexturePath });
                string[] texturePaths = textureGuids.Select(AssetDatabase.GUIDToAssetPath).ToArray();

                if (DisplayCancelableProgressBar(0.3f, "Loading Materials")) return;

                string absoluteMaterialPath = PathHelper.EnsurePathAsApplicationAbsolute(materialFolderPath);
                string unityMaterialPath = PathHelper.AbsolutePathToRelative(absoluteMaterialPath);
                string[] materialGuids = AssetDatabase.FindAssets("t:Material", new string[] { unityMaterialPath });
                string[] materialPaths = materialGuids.Select(AssetDatabase.GUIDToAssetPath).ToArray();

                if (DisplayCancelableProgressBar(0.5f, "Preparing Datas")) return;

                materialPath = unityMaterialPath;
                materialDatas = materialPaths.Select((string s) => {
                    return new MaterialData { name = PathHelper.PathToName(s, "mat"), path = s };
                }).ToList();

                if (DisplayCancelableProgressBar(0.6f, "Iterating Textures")) return;

                for (int i = 0; i < texturePaths.Length; ++i) {
                    string texturePath = texturePaths[i], texture = PathHelper.PathToName(texturePath);
                    TextureData textureData = new TextureData { name = texture, path = texturePath };

                    float advancement = 0.6f + i / texturePaths.Length * 0.4f;
                    if (DisplayCancelableProgressBar(advancement, $"Iterating Textures : {texture}")) return;

                    foreach (TextureMapping textureMapping in textureMapping.mappings) {
                        IterateTexture(textureMapping, textureData);
                    }
                }
            } catch { Debug.LogError($"{toolName} - Something Went Wrong while Importing"); }

            materialPath = string.Empty;
            materialDatas = new List<MaterialData>();
            DisplayCancelableProgressBar(1f, "Saving Assets");
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        #endregion ImportTextures

        ////////////////////////////////////////////
        ///////////// ITERATE TEXTURE //////////////
        ////////////////////////////////////////////

        #region IterateTexture

        private void IterateTexture(TextureMapping textureMapping, TextureData texture)
        {
            int mappingIndex = texture.name.IndexOf(textureMapping.name);
            if (mappingIndex >= 0) {
                List<string> split;
                string textureName = texture.name.Substring(0, mappingIndex);
                if (!string.IsNullOrEmpty(textureNameSeparator)) {
                    split = textureName.Split(textureNameSeparator).ToList();
                    if (string.IsNullOrEmpty(split[split.Count - 1])) split.RemoveAt(split.Count - 1);
                } else split = new List<string> { textureName };
                string correspondingName = string.Empty;
                int materialIndex = -1;

                for (int i = 0; i < split.Count; ++i) {
                    for (int j = split.Count; j > i; --j) {
                        List<string> range = split.GetRange(i, j - i);
                        correspondingName = string.Join(textureNameSeparator, range);
                        materialIndex = materialDatas.FindIndex((MaterialData mat) => {
                            return string.Equals(mat.name, correspondingName, StringComparison.OrdinalIgnoreCase);
                        });
                        if (materialIndex >= 0) break;
                    }
                    if (materialIndex >= 0) break;
                }

                Material material;
                if (materialIndex < 0) {
                    if (!createMaterial) return;
                    string materialName = string.Join(materialNameSeparator, split);
                    string path = $"{PathHelper.CombinePath(materialPath, materialName)}.mat";
                    material = CreateMaterial(materialName, path);
                    materialDatas.Add(new MaterialData { name = materialName, path = path });
                } else {
                    MaterialData materialData = materialDatas[materialIndex];
                    material = AssetDatabase.LoadAssetAtPath<Material>(materialData.path);
                }

                if (!material.HasProperty(textureMapping.property)) {
                    Debug.LogError($"Could not Find \"{textureMapping.property}\" on Material \"{material.name}\"");
                    return;
                }

                Texture loadedTexture = AssetDatabase.LoadAssetAtPath<Texture>(texture.path);
                material.SetTexture(textureMapping.property, loadedTexture);
                AssetDatabase.SaveAssetIfDirty(material);
            }
        }

        #endregion IterateTexture

        ////////////////////////////////////////////
        ///////////// CREATE MATERIAL //////////////
        ////////////////////////////////////////////

        #region CreateMaterial

        private Material CreateMaterial(string name, string path)
        {
            Shader shader;
            if (!path.EndsWith(".mat")) path = $"{path}.mat";
            if (textureMapping != null) {
                shader = Shader.Find(textureMapping.defaultShader);
                if (shader == null) {
                    Debug.LogWarning($"Shader \"{textureMapping.defaultShader}\" was not found. Falling back to Standard");
                    shader = Shader.Find(standardShader);
                }
            } else {
                RenderPipelineAsset renderPipeline = GraphicsSettings.defaultRenderPipeline;
                if (renderPipeline == null) {
                    Debug.LogWarning($"Default Render Pipeline was not found. Falling back to Standard");
                    shader = Shader.Find(standardShader);
                } else shader = renderPipeline.defaultShader;
            }

            Material material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
            Debug.Log($"{toolName} - Created Material {name} at path {path}");
            return material;
        }

        #endregion CreateMaterial

        ////////////////////////////////////////////
        ///// DISPLAY CANCELABLE PROGRESS BAR //////
        ////////////////////////////////////////////

        #region DisplayCancelableProgressBar

        private bool DisplayCancelableProgressBar(float advancement, string info)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Importing Textures", info, advancement)) {
                EditorUtility.ClearProgressBar();
                return true;
            }
            return false;
        }

        #endregion DisplayCancelableProgressBar
    }
}