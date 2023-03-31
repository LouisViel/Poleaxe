namespace Poleaxe.Editor.Utils.AssetProcessor
{
    internal class AssetPostprocessor : UnityEditor.AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] createdAssets, string[] deletedAssets, string[] movedAssets, string[] movedAssetsPrevious)
        {
            foreach (string asset in createdAssets) {
                AssetProcessing processing = new AssetProcessing { current = asset };
                if (AssetProcessor.AboutBeingCreated.Contains(asset)) {
                    AssetProcessor.AboutBeingCreated.Remove(asset);
                    AssetProcessor.AboutBeingSaved.Remove(asset);
                    AssetProcessor.OnCreate.Invoke(processing);
                }
                if (AssetProcessor.AboutBeingSaved.Contains(asset)) {
                    AssetProcessor.AboutBeingSaved.Remove(asset);
                    AssetProcessor.OnSave.Invoke(processing);
                }
            }

            foreach (string asset in deletedAssets) {
                AssetProcessor.OnDelete.Invoke(new AssetProcessing { current = asset });
            }

            for (int i = 0; i < movedAssets.Length; ++i) {
                AssetProcessor.OnMove.Invoke(new AssetProcessing {
                    current = movedAssets[i], previous = movedAssetsPrevious[i]
                });
            }
        }
    }
}