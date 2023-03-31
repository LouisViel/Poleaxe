using UnityEngine;

namespace Poleaxe.Utils.Attribute
{
    public class SceneAttribute : PropertyAttribute
    {
        public SceneMode SceneMode { get; private set; }
        public SceneAttribute() => SceneMode = SceneMode.Default;
        public SceneAttribute(SceneMode sceneMode) => SceneMode = sceneMode;
        public void SetSceneMode(SceneMode sceneMode) => SceneMode = sceneMode;
    }

    public enum SceneMode
    {
        Default,
        Error,
        Name,
        Path,
        BuildId,
    }
}