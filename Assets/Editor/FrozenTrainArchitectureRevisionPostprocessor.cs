#if UNITY_EDITOR
using System.Linq;
using UnityEditor;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Keeps the single Rebuild Authored Train workflow authoritative.
    /// Final architecture rules are re-applied automatically whenever the generated
    /// car/connector prefabs or playable scene are saved.
    /// </summary>
    public sealed class FrozenTrainArchitectureRevisionPostprocessor : AssetPostprocessor
    {
        private static bool applying;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (applying) return;

            string[] carPaths = importedAssets.Where(FrozenTrainArchitectureRevision.IsCarPath).Distinct().ToArray();
            bool connectorChanged = importedAssets.Contains(FrozenTrainArchitectureRevision.ConnectorPath);
            bool sceneChanged = importedAssets.Contains(FrozenTrainArchitectureRevision.ScenePath);

            if (carPaths.Length == 0 && !connectorChanged && !sceneChanged) return;

            applying = true;
            try
            {
                foreach (string path in carPaths)
                    FrozenTrainArchitectureRevision.ApplyCar(path);

                if (connectorChanged)
                    FrozenTrainArchitectureRevision.ApplyConnector();

                if (sceneChanged)
                    FrozenTrainArchitectureRevision.ApplySceneSnow();
            }
            finally
            {
                applying = false;
            }
        }
    }
}
#endif
