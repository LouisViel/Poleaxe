using System.Linq;
using UnityEditor;
using Poleaxe.Editor.Helper;
using Poleaxe.Editor.Utils.AssetProcessor;
using Poleaxe.Editor.Utils.ResourceCreator;
using Poleaxe.Utils;
using Poleaxe.Utils.Resource;
using Poleaxe.Utils.Event;

namespace Poleaxe.SceneManagement.Editor.Runtime
{
    public static class SceneMapper
    {
        private static SceneMapping sceneMapping;
        static ResourceCreatorData resourceCreator = new ResourceCreatorData {
            fileName = nameof(SceneMapping), containerDirectoryName = "Runtime"
        };        

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorBuildSettings.sceneListChanged += RefreshScenes;
            AssetProcessor.RegisterToAll(ManageProcessing);
            RefreshScenes();
        }

        private static void ManageProcessing(object _, EventData<AssetProcessing> data)
        {
            string current = data.Data.current, previous = data.Data.previous;
            if (current.EndsWith(PoleaxeConstants.SceneExtension) || previous.EndsWith(PoleaxeConstants.SceneExtension)) RefreshScenes();
        }

        private static void RefreshScenes()
        {
            ResourceCreator<SceneMapping>.Refresh(ref sceneMapping, resourceCreator);
            sceneMapping.scenes = EditorBuildSettings.scenes.Select((EditorBuildSettingsScene s) => {
                return new SceneMapped { name = PathHelper.PathToName(s.path, PoleaxeConstants.SceneExtension), path = s.path };
            }).ToList();
            ResourceCreator<SceneMapping>.Save(sceneMapping);
        }
    }
}