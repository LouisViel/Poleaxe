using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poleaxe.Utils.Resource
{
    [Serializable]
    public class SceneMapping : ScriptableObject
    {
        public List<SceneMapped> scenes = new List<SceneMapped>();
    }

    [Serializable]
    public class SceneMapped
    {
        public string name;
        public string path;
    }
}