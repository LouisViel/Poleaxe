using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Poleaxe.Utils.Resource;
using Poleaxe.Utils.Event;
using Poleaxe.Helper;

namespace Poleaxe.SceneManagement.Helper
{
    public static class SceneHelper
    {
        //////////////////////////////////////////////////
        //////////////////  STRUCTURES  //////////////////
        //////////////////////////////////////////////////

        private struct LoadDefered
        {
            public AsyncOperationLinked callback;
            public LoadSceneMode mode;
            public int index;
        }

        //////////////////////////////////////////////////
        ////////////////  PUBLIC FIELDS  /////////////////
        //////////////////////////////////////////////////
        
        public static bool IsAdditive => SceneManager.sceneCount > 1;
        public static bool IsDefaultAdditive { get; private set; } = false;

        private static int isLock = 0;
        public static bool IsLock => isLock > 0;

        //////////////////////////////////////////////////
        //////////////  PRIVATE VARIABLES  ///////////////
        //////////////////////////////////////////////////
        
        private static bool isLoadingScene = false;
        private static List<SceneMapped> scenes = new List<SceneMapped>();
        private static PoleaxeEventMultiple<object> OnUnlock = new PoleaxeEventMultiple<object>();

        //////////////////////////////////////////////////
        ///////////////  INITIALIZATION  /////////////////
        //////////////////////////////////////////////////

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            IsDefaultAdditive = IsAdditive;
            SceneMapping mapping = Resources.Load<SceneMapping>(nameof(SceneMapping));
            if (mapping != null) scenes = mapping.scenes;
        }

        //////////////////////////////////////////////////
        ////////////////  LOCK / UNLOCK  /////////////////
        //////////////////////////////////////////////////

        public static Action<AsyncOperation> SetLock() { ++isLock; return RemoveLock; }
        public static void RemoveLock(AsyncOperation _) => RemoveLock();
        public static void RemoveLock()
        {
            isLock = Mathf.Max(isLock - 1, 0);
            if (isLock == 0) OnUnlock.Invoke();
        }

        //////////////////////////////////////////////////
        ////////////////  RELOAD CURRENT  ////////////////
        //////////////////////////////////////////////////

        public static AsyncOperationLinked ReloadScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            return LoadScene(currentScene.buildIndex);
        }

        //////////////////////////////////////////////////
        /////////////////  LOAD SINGLE  //////////////////
        //////////////////////////////////////////////////

        public static AsyncOperationLinked LoadScene(string sceneName)
        {
            int sceneIndex = GetSceneBuildIndex(sceneName);
            if (sceneIndex <= 0) return null;
            return LoadScene(sceneIndex);
        }

        public static AsyncOperationLinked LoadScene(int sceneIndex)
        {
            if (isLoadingScene) return null;
            isLoadingScene = true;

            AsyncOperationLinked operation = InternalLoad(sceneIndex, LoadSceneMode.Single);
            operation.completed += InternalResetSettings;

            return operation;
        }

        //////////////////////////////////////////////////
        ////////////////  LOAD ADDITIVE  /////////////////
        //////////////////////////////////////////////////

        public static AsyncOperationLinked LoadSceneAdditive(string sceneName)
        {
            int sceneIndex = GetSceneBuildIndex(sceneName);
            if (sceneIndex <= 0) return null;
            return LoadSceneAdditive(sceneIndex);
        }

        public static AsyncOperationLinked LoadSceneAdditive(int sceneIndex)
        {
            if (isLoadingScene) return null;
            return InternalLoad(sceneIndex, LoadSceneMode.Additive);
        }

        //////////////////////////////////////////////////
        ////////////////  GET SCENE DATA  ////////////////
        //////////////////////////////////////////////////

        public static int GetSceneBuildIndex(string sceneString)
        {
            return scenes.FindIndex((SceneMapped s) => s.name == sceneString || s.path == sceneString);
        }

        //////////////////////////////////////////////////
        //////////////////  GET SCENE  ///////////////////
        //////////////////////////////////////////////////

        public static Scene? GetSceneByString(string sceneString)
        {
            bool getByPath = sceneString.Contains(Path.DirectorySeparatorChar) || sceneString.Contains(Path.AltDirectorySeparatorChar);
            try { Path.GetFullPath(sceneString); } catch { getByPath = false; }
            return getByPath ? GetSceneByPath(sceneString) : GetSceneByName(sceneString);
        }

        public static Scene? GetSceneByBuildIndex(int sceneIndex)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            if (!scene.IsValid()) return null;
            return scene;
        }

        public static Scene? GetSceneByPath(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid()) return null;
            return scene;
        }

        public static Scene? GetSceneByName(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) return null;
            return scene;
        }

        //////////////////////////////////////////////////
        ////////////////  INTERNAL LOAD  /////////////////
        //////////////////////////////////////////////////

        private static AsyncOperationLinked InternalLoad(int index, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single && IsLock) return InternalLoadLock(index, mode);
            return new AsyncOperationLinked(SceneManager.LoadSceneAsync(index, mode));
        }

        private static AsyncOperationLinked InternalLoadLock(int index, LoadSceneMode mode)
        {
            AsyncOperationLinked callback = new AsyncOperationLinked();
            LoadDefered loadUnlock = new LoadDefered { callback = callback, mode = mode, index = index };
            OnUnlock.Register(null, () => InternalLoadDefered(loadUnlock), true);
            return callback;
        }

        //////////////////////////////////////////////////
        //////////////  INTERNAL CALLBACK  ///////////////
        //////////////////////////////////////////////////

        private static void InternalLoadDefered(LoadDefered loadUnlock)
        {
            AsyncOperationLinked operation = InternalLoad(loadUnlock.index, loadUnlock.mode);
            loadUnlock.callback.LinkOperation(operation);
        }

        private static void InternalResetSettings(AsyncOperation _)
        {
            isLoadingScene = false;
        }
    }
}