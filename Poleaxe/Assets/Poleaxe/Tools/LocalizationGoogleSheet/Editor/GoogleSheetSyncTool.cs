#if LOCALIZATION
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.UI;
using UnityEngine;
using Poleaxe.Editor.Helper;
using Poleaxe.Utils;

namespace Poleaxe.LGS.Editor
{
    [Serializable]
    public class GoogleSheetSyncTool : EditorWindow, IHasCustomMenu
    {
        ////////////////////////////////////////////
        /////////////// STRUCTURES /////////////////
        ////////////////////////////////////////////

        #region Structures

        private class GoogleSheetTable
        {
            public string name;
            public StringTableCollection table;
            public GoogleSheetsExtension extension;
            public bool canPush;
        }

        #endregion Structures

        ///////////////////////////////////////////////
        ////////////////  CONSTANTS  //////////////////
        ///////////////////////////////////////////////

        #region Constants

        const string readableToolName = "Google Sheet Sync";

        static readonly string toolName = readableToolName.Replace(" ", "");
        static readonly Type type = typeof(GoogleSheetSyncTool);

        #endregion Constants

        ///////////////////////////////////////////////
        /////////////  STATIC VARIABLES  //////////////
        ///////////////////////////////////////////////

        #region StaticVariables

        [MenuItem(PoleaxeConstants.ToolsLocation + "/" + readableToolName)]
        public static GoogleSheetSyncTool OpenPrefabImporter() => GetWindow<GoogleSheetSyncTool>(readableToolName);
        public static GoogleSheetSyncTool Instance;

        #endregion StaticVariables

        ////////////////////////////////////////////
        //////////////// VARIABLES  ////////////////
        ////////////////////////////////////////////

        #region Variables

        private SerializedObject so;
        private Vector2 scrollPosition;
        private GoogleSheetTable[] sheetTables;
        public bool AutoOpenTables { get; set; }
        public bool AutoRunBuild { get; set; }

        #endregion Variables

        ////////////////////////////////////////////
        //////////////// ON ENABLE /////////////////
        ////////////////////////////////////////////

        #region OnEnable

        void OnEnable()
        {
            Instance = this;
            so = new SerializedObject(this);
            EditorApplication.projectChanged += OnProjectChanged; // Change to Use AssetProcessor to avoir useless Processing
            Undo.undoRedoPerformed += OnUndoRedo;
            GetStringTables();
            LoadSettings();
        }

        #endregion OnEnable

        ////////////////////////////////////////////
        /////////////// ON DISABLE /////////////////
        ////////////////////////////////////////////

        #region OnDisable

        void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
            SaveSettings();
        }

        #endregion OnDisable

        ////////////////////////////////////////////
        ///////////// LOAD SETTINGS ////////////////
        ////////////////////////////////////////////

        #region LoadSettings

        private void LoadSettings()
        {
            AutoOpenTables = EditorPrefs.GetBool($"{toolName}.AutoOpenTables", false);
            AutoRunBuild = EditorPrefs.GetBool($"{toolName}.AutoRunBuild", false);
        }

        #endregion LoadSettings

        ////////////////////////////////////////////
        ///////////// SAVE SETTINGS ////////////////
        ////////////////////////////////////////////

        #region SaveSettings

        private void SaveSettings()
        {
            EditorPrefs.SetBool($"{toolName}.AutoOpenTables", AutoOpenTables);
            EditorPrefs.SetBool($"{toolName}.AutoRunBuild", AutoRunBuild);
        }

        #endregion SaveSettings

        ////////////////////////////////////////////
        /////////// ON PROJECT CHANGE //////////////
        ////////////////////////////////////////////

        #region OnProjectChanged

        void OnProjectChanged()
        {
            GetStringTables();
            Repaint();
        }

        #endregion OnProjectChanged

        ////////////////////////////////////////////
        ///////////// ON UNDO REDO /////////////////
        ////////////////////////////////////////////

        #region OnUndoRedo

        void OnUndoRedo()
        {
            if (UndoHelper.HasChanged()) {
                if (UndoHelper.TryGetData(out UndoHelper.UndoData data)) {
                    switch (data.key) {
                        case "PropertyBool": {
                            string[] propertyAndValue = data.value.Split('=');
                            PropertyInfo info = type.GetProperty(propertyAndValue[0]);
                            info.SetValue(this, bool.Parse(propertyAndValue[1]));
                            UndoHelper.ConsumeChangement();
                            break;
                        }
                        default: break;
                    }
                }
            }
        }

        #endregion OnUndoRedo

        ////////////////////////////////////////////
        ///////////////// ON GUI ///////////////////
        ////////////////////////////////////////////

        #region OnGUI

        void OnGUI()
        {
            so.Update();
            bool buildAdressable = false;
            bool buildAdressableAndPlayer = false;

            GUIStyle style = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleCenter, fontSize = 22 };
            GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(25f) };
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition)) {
                scrollPosition = scrollView.scrollPosition;

                // PULL TABLES
                using (new EditorGUILayout.VerticalScope()) {
                    GUILayout.Space(2);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Pull Tables", style, options);
                        GUILayout.Space(2f);
                        for (int i = 0; i < sheetTables.Length; ++i) {
                            string name = $"Pull {sheetTables[i].name}";
                            if (GUILayout.Button(name)) PullTable(sheetTables[i]);
                        }
                        GUILayout.FlexibleSpace();
                    }
                }

                // PUSH TABLES
                using (new EditorGUILayout.VerticalScope()) {
                    GUILayout.Space(2);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Push Tables", style, options);
                        GUILayout.Space(2f);
                        for (int i = 0; i < sheetTables.Length; ++i) {
                            GUI.enabled = sheetTables[i].canPush;
                            string name = $"Push {sheetTables[i].name}";
                            if (GUILayout.Button(name)) PushTable(sheetTables[i]);
                        }
                        GUI.enabled = true;
                        GUILayout.FlexibleSpace();
                    }
                }

                // BUILD TABLES
                using (EditorGUILayout.VerticalScope scope1 = new EditorGUILayout.VerticalScope()) {
                    GUILayout.Space(2);
                    using (EditorGUILayout.VerticalScope scope2 = new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                        GUILayout.Space(2f);
                        EditorGUILayout.LabelField("Build Tables", style, options);
                        GUILayout.Space(1f);
                        if (GUILayout.Button("Build Tables")) {
                            buildAdressable = true;
                            /*scope1.Dispose();
                            scope2.Dispose();
                            AdressablesHelper.BuildAddressables();*/
                        }
                        if (GUILayout.Button("Build Tables And Player")) {
                            buildAdressableAndPlayer = true;
                            /*scope1.Dispose();
                            scope2.Dispose();
                            AdressablesHelper.BuildAddressablesAndPlayer(AutoRunBuild);*/
                        }
                        GUILayout.Space(2f);
                    }
                }
            }
            so.ApplyModifiedProperties();


            // UNFOCUS
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                GUI.FocusControl(null);
                Repaint();
            }

            // APPLY DEFERED BUILD TABLES
            if (buildAdressableAndPlayer) AdressablesHelper.BuildAddressablesAndPlayer(AutoRunBuild);
            else if (buildAdressable) AdressablesHelper.BuildAddressables();
        }

        #endregion OnGUI

        ////////////////////////////////////////////
        /////////// GET STRING TABLES //////////////
        ////////////////////////////////////////////

        #region GetStringTables

        private void GetStringTables()
        {
            string[] guids = AssetDatabase.FindAssets("t:StringTableCollection");
            IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
            StringTableCollection[] tables = paths.Select(AssetDatabase.LoadAssetAtPath<StringTableCollection>).ToArray();
            sheetTables = GetGoogleSheetTables(tables);
        }

        #endregion GetStringTables

        ////////////////////////////////////////////
        ///////// GET GOOGLE SHEET TABLES //////////
        ////////////////////////////////////////////

        #region GetGoogleSheetTables

        private GoogleSheetTable[] GetGoogleSheetTables(StringTableCollection[] tables)
        {
            return tables.Select((StringTableCollection table) => {
                GoogleSheetsExtension extension = GetGoogleSheetsExtension(table);
                if (extension == default) return default;
                return new GoogleSheetTable {
                    name = table.name,
                    table = table,
                    extension = extension,
                    canPush = extension.SheetsServiceProvider.Authentication == AuthenticationType.OAuth
                };
            }).Where((GoogleSheetTable table) => table != default).ToArray();
        }

        #endregion GetGoogleSheetTables

        ////////////////////////////////////////////
        ////////// GET SHEET EXTENSIONS ////////////
        ////////////////////////////////////////////

        #region GetGoogleSheetsExtension

        private GoogleSheetsExtension GetGoogleSheetsExtension(StringTableCollection table)
        {
            return table.Extensions.Select((CollectionExtension ext) => (GoogleSheetsExtension)ext).Where((GoogleSheetsExtension extension) => {
                return extension != null && extension.SheetsServiceProvider != null && !string.IsNullOrEmpty(extension.SpreadsheetId);
            }).ElementAtOrDefault(0);
        }

        #endregion GetGoogleSheetsExtension

        ////////////////////////////////////////////
        /////////////// PULL TABLE /////////////////
        ////////////////////////////////////////////

        #region PullTable

        private void PullTable(GoogleSheetTable table)
        {
            GoogleSheetsExtension extension = table.extension;
            GoogleSheets sheets = new GoogleSheets(extension.SheetsServiceProvider) { SpreadSheetId = extension.SpreadsheetId };
            sheets.PullIntoStringTableCollection(extension.SheetId, table.table, extension.Columns, extension.RemoveMissingPulledKeys);
            if (AutoOpenTables) LocalizationTablesWindow.ShowWindow(table.table);
        }

        #endregion PullTable

        ////////////////////////////////////////////
        /////////////// PUSH TABLE /////////////////
        ////////////////////////////////////////////

        #region PushTable

        private void PushTable(GoogleSheetTable table)
        {
            GoogleSheetsExtension extension = table.extension;
            GoogleSheets sheets = new GoogleSheets(extension.SheetsServiceProvider) { SpreadSheetId = extension.SpreadsheetId };
            sheets.PushStringTableCollection(extension.SheetId, table.table, extension.Columns);
        }

        #endregion PushTable

        ////////////////////////////////////////////
        /////////// ADD ITEMS TO MENU  /////////////
        ////////////////////////////////////////////

        #region AddItemsToMenu

        public void AddItemsToMenu(GenericMenu menu)
        {
            string _autoOpenTables = (AutoOpenTables ? "Disable" : "Enable") + " autoOpenTables";
            string _autoRunBuild = (AutoRunBuild ? "Disable" : "Enable") + " autoRunBuild";
            menu.AddItem(EditorGUIUtility.TrTextContent(_autoOpenTables), false, TriggerAutoOpenTables);
            menu.AddItem(EditorGUIUtility.TrTextContent(_autoRunBuild), false, TriggerAutoRunBuild);
        }

        #endregion AddItemsToMenu

        ////////////////////////////////////////////
        //////// TRIGGER AUTO OPEN TABLES //////////
        ////////////////////////////////////////////

        #region TriggerAutoOpenTables

        private void TriggerAutoOpenTables()
        {
            string title = $"[{readableToolName}] Trigger AutoOpenTables";
            UndoHelper.Save(title, "PropertyBool", $"autoOpenTables={AutoOpenTables}");
            AutoOpenTables = !AutoOpenTables;
        }

        #endregion TriggerAutoOpenTables

        ////////////////////////////////////////////
        ///////// TRIGGER AUTO RUN BUILD ///////////
        ////////////////////////////////////////////

        #region TriggerAutoRunBuild

        private void TriggerAutoRunBuild()
        {
            string title = $"[{readableToolName}] Trigger AutoRunBuild";
            UndoHelper.Save(title, "PropertyBool", $"autoRunBuild={AutoRunBuild}");
            AutoRunBuild = !AutoRunBuild;
        }

        #endregion TriggerAutoRunBuild
    }
}
#endif