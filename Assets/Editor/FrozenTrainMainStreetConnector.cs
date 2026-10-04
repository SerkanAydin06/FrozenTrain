#if UNITY_EDITOR
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    internal static class FrozenTrainMainStreetConnector
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string ConnectorPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";
        private const float HalfCarWidth = 4.60f;

        public static void Apply()
        {
            foreach (CarSpec car in FrozenTrainLayout.Cars)
            {
                string path = PrefabRoot + "/" + car.Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    ShiftEndOpening(root.transform, "MainFront");
                    ShiftEndOpening(root.transform, "MainRear");
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            ShiftConnector();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ShiftEndOpening(Transform root, string prefix)
        {
            Transform left = FindFirst(root, prefix + "L");
            Transform right = FindFirst(root, prefix + "R");
            Transform lintel = FindFirst(root, prefix + "Lintel");
            if (left == null || right == null || lintel == null) return;

            float z0 = FrozenTrainMainStreetArchitecture.CorridorMinZ;
            float z1 = FrozenTrainMainStreetArchitecture.CorridorMaxZ;
            float openingWidth = z1 - z0;
            float leftWidth = z0 + HalfCarWidth;
            float rightWidth = HalfCarWidth - z1;

            SetStrip(left, leftWidth, (-HalfCarWidth + z0) * .5f);
            SetStrip(right, rightWidth, (z1 + HalfCarWidth) * .5f);

            Vector3 lp = lintel.localPosition;
            lp.z = (z0 + z1) * .5f;
            lintel.localPosition = lp;
            Vector3 ls = lintel.localScale;
            ls.z = openingWidth;
            lintel.localScale = ls;
        }

        private static void SetStrip(Transform t, float width, float z)
        {
            Vector3 p = t.localPosition; p.z = z; t.localPosition = p;
            Vector3 s = t.localScale; s.z = Mathf.Max(.08f, width); t.localScale = s;
        }

        private static void ShiftConnector()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                float z0 = FrozenTrainMainStreetArchitecture.CorridorMinZ;
                float z1 = FrozenTrainMainStreetArchitecture.CorridorMaxZ;
                float center = FrozenTrainMainStreetArchitecture.CorridorCenterZ;
                float width = FrozenTrainMainStreetArchitecture.CorridorWidth;

                Transform mainFloor = FindFirst(root.transform, "MainFloor");
                if (mainFloor != null)
                {
                    Vector3 p = mainFloor.localPosition; p.z = center; mainFloor.localPosition = p;
                    Vector3 s = mainFloor.localScale; s.z = width; mainFloor.localScale = s;
                }

                Transform left = FindFirst(root.transform, "MainBellowsL");
                Transform right = FindFirst(root.transform, "MainBellowsR");
                if (left != null) { Vector3 p = left.localPosition; p.z = z0; left.localPosition = p; }
                if (right != null) { Vector3 p = right.localPosition; p.z = z1; right.localPosition = p; }

                Transform doorRoot = FindFirst(root.transform, "SlidingDoor");
                if (doorRoot != null)
                {
                    Vector3 p = doorRoot.localPosition; p.z = center; doorRoot.localPosition = p;
                    SlidingDoor door = doorRoot.GetComponent<SlidingDoor>();
                    if (door != null) door.Configure(new Vector3(0f, 0f, -2.35f), 2.8f);
                }

                Transform panel = FindFirst(root.transform, "DoorPanel");
                if (panel != null) { Vector3 s = panel.localScale; s.z = width - .22f; panel.localScale = s; }

                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
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
