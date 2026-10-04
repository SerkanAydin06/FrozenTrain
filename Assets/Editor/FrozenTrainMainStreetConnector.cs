#if UNITY_EDITOR
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>Widens the existing main-deck end openings and shared connector to match Main Street.</summary>
    internal static class FrozenTrainMainStreetConnector
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string ConnectorPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";
        private const float OpeningWidth = 2.80f;
        private const float HalfCarWidth = 4.60f;

        public static void Apply()
        {
            foreach (FrozenTrainLayout.CarSpec car in FrozenTrainLayout.Cars)
            {
                string path = PrefabRoot + "/" + car.Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    WidenEnd(root.transform, "MainFront");
                    WidenEnd(root.transform, "MainRear");
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            WidenConnector();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void WidenEnd(Transform root, string prefix)
        {
            Transform left = FindFirst(root, prefix + "L");
            Transform right = FindFirst(root, prefix + "R");
            Transform lintel = FindFirst(root, prefix + "Lintel");
            if (left == null || right == null || lintel == null) return; // closed train ends have no opening pieces

            float sideWidth = (HalfCarWidth * 2f - OpeningWidth) * .5f;
            float centre = OpeningWidth * .5f + sideWidth * .5f;

            SetWidthAndZ(left, sideWidth, -centre);
            SetWidthAndZ(right, sideWidth, centre);
            Vector3 lintelScale = lintel.localScale;
            lintelScale.z = OpeningWidth;
            lintel.localScale = lintelScale;
        }

        private static void SetWidthAndZ(Transform t, float width, float z)
        {
            Vector3 p = t.localPosition;
            p.z = z;
            t.localPosition = p;
            Vector3 s = t.localScale;
            s.z = width;
            t.localScale = s;
        }

        private static void WidenConnector()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                Transform mainFloor = FindFirst(root.transform, "MainFloor");
                if (mainFloor != null)
                {
                    Vector3 s = mainFloor.localScale;
                    s.z = OpeningWidth;
                    mainFloor.localScale = s;
                }

                Transform bellowsL = FindFirst(root.transform, "MainBellowsL");
                Transform bellowsR = FindFirst(root.transform, "MainBellowsR");
                if (bellowsL != null)
                {
                    Vector3 p = bellowsL.localPosition;
                    p.z = -OpeningWidth * .5f;
                    bellowsL.localPosition = p;
                }
                if (bellowsR != null)
                {
                    Vector3 p = bellowsR.localPosition;
                    p.z = OpeningWidth * .5f;
                    bellowsR.localPosition = p;
                }

                Transform panel = FindFirst(root.transform, "DoorPanel");
                if (panel != null)
                {
                    Vector3 s = panel.localScale;
                    s.z = 2.38f;
                    panel.localScale = s;
                }

                Transform doorRoot = FindFirst(root.transform, "SlidingDoor");
                if (doorRoot != null)
                {
                    SlidingDoor door = doorRoot.GetComponent<SlidingDoor>();
                    if (door != null) door.Configure(new Vector3(0f, 0f, -2.58f), 2.8f);
                }

                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform FindFirst(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = FindFirst(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
