using System;
using UnityEditor;
using UnityEngine;

namespace Poleaxe.Editor.Helper
{
    public static class UndoHelper
    {
        ///////////////////////////////////////////////
        ////////////////  STRUCTURES  /////////////////
        ///////////////////////////////////////////////

        #region Structures

        [Serializable]
        internal class SerializableUndoData : ScriptableObject
        {
            public int index = 0;
            public string key = null;
            public string value = null;
        }

        public class UndoData
        {
            public readonly int index;
            public readonly string key;
            public readonly string value;

            internal UndoData(SerializableUndoData data)
            {
                index = data.index;
                key = data.key;
                value = data.value;
            }
        }

        #endregion Structures

        ///////////////////////////////////////////////
        /////////////////  VARIABLES  /////////////////
        ///////////////////////////////////////////////

        #region Variables

        private static int currentIndex = 0;
        private static SerializableUndoData undoRedoSave = ScriptableObject.CreateInstance<SerializableUndoData>();
        private static SerializableUndoData currentSave = ScriptableObject.CreateInstance<SerializableUndoData>();

        #endregion Variables

        ///////////////////////////////////////////////
        ///////////////////  SAVE  ////////////////////
        ///////////////////////////////////////////////

        #region Save

        public static void Save(string action, string key, string value)
        {
            ConsumeChangement();
            Undo.RecordObject(undoRedoSave, action);
            undoRedoSave.index = ++currentIndex;
            undoRedoSave.key = key;
            undoRedoSave.value = value;
            CopySave();
        }

        #endregion Save

        ///////////////////////////////////////////////
        ////////////////  HAS CHANGED  ////////////////
        ///////////////////////////////////////////////

        #region HasChanged

        public static bool HasChanged()
        {
            return undoRedoSave.index != currentIndex;
        }

        #endregion HasChanged

        ///////////////////////////////////////////////
        ///////////////  TRY GET DATA  ////////////////
        ///////////////////////////////////////////////

        #region TryGetData

        public static bool TryGetData(out UndoData undoData)
        {
            if (undoRedoSave.index > currentIndex) {
                undoData = new UndoData(undoRedoSave);
                return true;
            }

            if (undoRedoSave.index < currentIndex) {
                undoData = new UndoData(currentSave);
                return true;
            }

            undoData = null;
            return false;
        }

        #endregion TryGetData

        ///////////////////////////////////////////////
        ////////////  CONSUME CHANGEMENT  /////////////
        ///////////////////////////////////////////////

        #region ConsumeChangement

        public static void ConsumeChangement()
        {
            currentIndex = undoRedoSave.index;
            CopySave();
        }

        #endregion ConsumeChangement

        ///////////////////////////////////////////////
        ////////////////  COPY SAVE  //////////////////
        ///////////////////////////////////////////////

        #region CopySave

        private static void CopySave()
        {
            currentSave.index = undoRedoSave.index;
            currentSave.key = undoRedoSave.key;
            currentSave.value = undoRedoSave.value;
        }

        #endregion CopySave
    }
}