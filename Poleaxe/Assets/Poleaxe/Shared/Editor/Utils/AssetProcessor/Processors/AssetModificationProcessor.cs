using Poleaxe.Editor.Helper;

namespace Poleaxe.Editor.Utils.AssetProcessor
{
    internal class AssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        static void OnWillCreateAsset(string assetPath)
        {
            if (assetPath.EndsWith(".meta")) {
                string path = PathHelper.PathWithoutExtension(assetPath, ".meta");
                if (AssetProcessor.AboutBeingCreated.Contains(path)) return;
                AssetProcessor.AboutBeingCreated.Add(path);
            } else AssetProcessor.AboutBeingCreated.Add(assetPath);
        }

        static string[] OnWillSaveAssets(string[] assetPaths)
        {
            foreach (string path in assetPaths) {
                if (AssetProcessor.AboutBeingSaved.Contains(path)) continue;
                AssetProcessor.AboutBeingSaved.Add(path);
            }
            return assetPaths;
        }

        // static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        // {
        //     // INFO : AssetDatabase cannot be called from here
        //     return AssetDeleteResult.DidNotDelete;
        // }

        // static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        // {
        //     // INFO : AssetDatabase cannot be called from here
        //     return AssetMoveResult.DidNotMove;
        // }
    }
}