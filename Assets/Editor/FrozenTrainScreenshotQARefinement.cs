#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Final visual QA pass driven by the automatic screenshots. It only improves presentation and
    /// ergonomics after the canonical builder / production-art / upper-circulation passes have run.
    /// Gameplay bindings stay on the original authored roots.
    /// </summary>
    internal static class FrozenTrainScreenshotQARefinement
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MeshRoot = "Assets/Art/Meshes/Generated";
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string MarkerName = "__ScreenshotQAV1";

        private sealed class Mats
        {
            public Material Floor;
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material Fabric;
            public Material Hazard;
            public Material Generator;
            public Material Workshop;
            public Material Green;
            public Material Screen;
            public Material Warm;
            public Material Exterior;
            public Material Snow;
            public Material Rock;
        }

        public static void Apply()
        {
            Mats mats = LoadMaterials();
            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                RefineCar(path, i, mats);
            }

            RefineExteriorScene(mats);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void RefineCar(string path, int index, Mats m)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform old = root.transform.Find(MarkerName);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                Transform marker = Group(root.transform, MarkerName, Vector3.zero);

                RefineLowerDeck(root.transform, marker, index, m);
                RefineMainDeck(root.transform, marker, index, m);
                RefineUpperDeck(root.transform, marker, index, m);
                RefineExterior(root.transform, marker, index, m);

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // ---- Lower technical decks ----------------------------------------------------------

        private static void RefineLowerDeck(Transform root, Transform marker, int index, Mats m)
        {
            Transform q = Group(marker, "LowerDeckQA", Vector3.zero);
            float y = L.LowerTop + .018f;

            // The screenshots read as one dark machinery block. Give every technical deck an obvious
            // central service aisle, edge markings and overhead service lights instead.
            Cube(q, "ServiceAisleRunner", new Vector3(0f, y, 0f), new Vector3(L.CarLength - 1.0f, .024f, 1.82f), m.Floor, false);
            Cube(q, "ServiceAisleEdgeL", new Vector3(0f, y + .012f, -.98f), new Vector3(L.CarLength - 1.0f, .025f, .055f), m.Hazard, false);
            Cube(q, "ServiceAisleEdgeR", new Vector3(0f, y + .012f, .98f), new Vector3(L.CarLength - 1.0f, .025f, .055f), m.Hazard, false);

            foreach (float x in new[] { -5.8f, 0f, 5.8f })
            {
                Cube(q, "LowerCeilingLuminaire", new Vector3(x, L.MainY - .28f, 0f), new Vector3(2.2f, .055f, .22f), m.Warm, false);
                Point(q, "LowerServiceLight", new Vector3(x, L.MainY - .42f, 0f), .72f, 5.2f, new Color(.62f, .76f, .88f));
            }

            Material accent = AccentFor(index, m);
            foreach (float x in new[] { -6.3f, 6.3f })
                Cube(q, "TechnicalZonePlate", new Vector3(x, L.LowerTop + .52f, -4.36f), new Vector3(1.15f, .38f, .035f), accent, false);
        }

        // ---- Main decks ---------------------------------------------------------------------

        private static void RefineMainDeck(Transform root, Transform marker, int index, Mats m)
        {
            Transform q = Group(marker, "MainDeckQA", Vector3.zero);
            Material accent = AccentFor(index, m);

            // Common centre circulation cue. It is intentionally narrow enough not to become a fake
            // second floor and makes the 18 m cars read as designed spaces rather than empty boxes.
            Cube(q, "MainAisleGuide", new Vector3(0f, L.MainTop + .016f, 0f), new Vector3(L.CarLength - 1.2f, .022f, 1.55f), m.Floor, false);
            Cube(q, "MainAisleEdgeL", new Vector3(0f, L.MainTop + .028f, -.83f), new Vector3(L.CarLength - 1.2f, .018f, .045f), accent, false);
            Cube(q, "MainAisleEdgeR", new Vector3(0f, L.MainTop + .028f, .83f), new Vector3(L.CarLength - 1.2f, .018f, .045f), accent, false);

            foreach (float x in new[] { -6.4f, 6.4f })
            {
                Cube(q, "CarRolePlate", new Vector3(x, 2.63f, 4.43f), new Vector3(1.35f, .30f, .045f), accent, false);
                Cube(q, "CarRolePlate", new Vector3(x, 2.63f, -4.43f), new Vector3(1.35f, .30f, .045f), accent, false);
            }

            switch (index)
            {
                case 0: RefineLocomotive(root, q, m); break;
                case 1: RefineGenerator(root, q, m); break;
                case 2: RefineLiving(root, q, m); break;
                case 3: RefineWorkshop(root, q, m); break;
                case 4: RefineGreenhouse(root, q, m); break;
            }
        }

        private static void RefineLocomotive(Transform root, Transform q, Mats m)
        {
            // The long empty approach to the cab in the screenshots gets two low side service consoles;
            // they stay outside the 1.7 m centre aisle and keep the panoramic sightline clear.
            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * 3.58f;
                Cube(q, "CabSideConsole", new Vector3(-1.55f, L.MainTop + .56f, z), new Vector3(3.15f, .92f, .72f), m.Frame, false);
                Cube(q, "CabSideConsoleTop", new Vector3(-1.55f, L.MainTop + 1.04f, z - side * .06f), new Vector3(3.18f, .07f, .78f), m.Steel, false);
                for (int i = -1; i <= 1; i++)
                    Cube(q, "CabStatusScreen", new Vector3(-1.55f + i * .88f, L.MainTop + 1.25f, z - side * .41f), new Vector3(.62f, .32f, .035f), m.Screen, false);
            }

            Cube(q, "CabOverheadBrow", new Vector3(-5.65f, 2.62f, 0f), new Vector3(.38f, .20f, 7.35f), m.Frame, false);
            Point(q, "CabFillLight", new Vector3(-5.15f, 2.55f, 0f), 1.35f, 6.0f, new Color(.78f, .86f, .94f));
        }

        private static void RefineGenerator(Transform root, Transform q, Mats m)
        {
            MoveLocal(root, "OperatorDesk", null, -3.88f, null);
            MoveLocal(root, "GeneratorConsole", null, -4.05f, null);
            MoveLocal(root, "MonitoringRack", null, 3.96f, null);
            MoveLocal(root, "ControlTable", null, -3.86f, null);

            // Two clear operator stations replace the empty centre-field feeling without blocking transit.
            Chair(q, "GeneratorOperatorSeatA", new Vector3(-2.75f, L.MainTop, -2.78f), 180f, m);
            Chair(q, "GeneratorOperatorSeatB", new Vector3(3.00f, L.MainTop, -2.78f), 180f, m);

            foreach (float x in new[] { -4.6f, .2f, 4.8f })
            {
                Cube(q, "GeneratorCableTray", new Vector3(x, 2.74f, 3.78f), new Vector3(3.6f, .10f, .32f), m.Frame, false);
                Cube(q, "GeneratorStatusBar", new Vector3(x, 2.35f, 4.34f), new Vector3(2.5f, .10f, .04f), m.Hazard, false);
            }
            Point(q, "GeneratorControlLightA", new Vector3(-2.8f, 2.40f, -2.8f), 1.05f, 5.0f, new Color(.62f, .78f, .95f));
            Point(q, "GeneratorControlLightB", new Vector3(3.0f, 2.40f, -2.8f), 1.05f, 5.0f, new Color(.62f, .78f, .95f));
        }

        private static void RefineLiving(Transform root, Transform q, Mats m)
        {
            foreach (Transform bay in FindAll(root, "SeatingBay"))
            {
                Vector3 p = bay.localPosition;
                p.z = Mathf.Sign(p.z == 0f ? 1f : p.z) * 3.32f;
                bay.localPosition = p;
            }

            // The white medical cabinet was visually sitting in the aisle. Move it onto the rear bulkhead
            // and turn its face toward the public space, keeping the E interaction on the same object.
            Transform med = FindFirst(root, "MedicalCabinet");
            if (med != null)
            {
                med.localPosition = new Vector3(7.66f, med.localPosition.y, 2.55f);
                med.localRotation = Quaternion.Euler(0f, 90f, 0f);
            }

            Transform galley = FindFirst(root, "GalleyCounter");
            Transform galleyTop = FindFirst(root, "GalleyTop");
            if (galley != null) galley.localPosition = new Vector3(galley.localPosition.x, galley.localPosition.y, -3.88f);
            if (galleyTop != null) galleyTop.localPosition = new Vector3(galleyTop.localPosition.x, galleyTop.localPosition.y, -3.88f);
            Transform galleyDetails = FindFirst(root, "__Production_GalleyDetails");
            if (galleyDetails != null) galleyDetails.localPosition = new Vector3(galleyDetails.localPosition.x, galleyDetails.localPosition.y, -3.88f);

            // Small public-service zone at the rear instead of loose furniture in the circulation path.
            Cube(q, "LivingServiceBackboard", new Vector3(7.78f, 1.48f, 0f), new Vector3(.08f, 2.45f, 4.15f), m.Wall, false);
            Cube(q, "LivingServiceHeader", new Vector3(7.72f, 2.58f, 0f), new Vector3(.16f, .12f, 4.20f), m.Fabric, false);
            Point(q, "LivingWarmFill", new Vector3(1.0f, 2.45f, 0f), 1.0f, 6.0f, new Color(1f, .78f, .56f));
        }

        private static void RefineWorkshop(Transform root, Transform q, Mats m)
        {
            foreach (Transform bench in FindAll(root, "Workbench"))
            {
                Vector3 p = bench.localPosition;
                p.z = Mathf.Sign(p.z == 0f ? 1f : p.z) * 3.72f;
                bench.localPosition = p;
            }

            Transform machine = FindFirst(root, "MachineStation");
            if (machine != null) machine.localPosition = new Vector3(5.20f, machine.localPosition.y, 3.42f);
            Transform machineScreen = FindFirst(root, "MachineScreen");
            if (machineScreen != null) machineScreen.localPosition = new Vector3(5.20f, machineScreen.localPosition.y, 2.70f);
            Transform machineDetails = FindFirst(root, "__Production_MachineStationDetails");
            if (machineDetails != null) machineDetails.localPosition = new Vector3(5.20f, machineDetails.localPosition.y, 3.42f);

            MoveLocal(root, "ToolCabinet", null, 3.92f, null);

            // Task-lighting and shallow wall storage make the workshop dense without sacrificing aisle width.
            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * 4.12f;
                foreach (float x in new[] { -5.5f, -.5f, 4.5f })
                {
                    Cube(q, "WorkshopPartsBin", new Vector3(x, 2.10f, z), new Vector3(1.42f, .34f, .30f), m.Workshop, false);
                    Cube(q, "WorkshopTaskLight", new Vector3(x, 2.72f, side * 3.45f), new Vector3(1.75f, .055f, .18f), m.Warm, false);
                }
            }
            Point(q, "WorkshopFillA", new Vector3(-3.2f, 2.45f, 0f), .95f, 5.2f, new Color(.95f, .76f, .48f));
            Point(q, "WorkshopFillB", new Vector3(3.2f, 2.45f, 0f), .95f, 5.2f, new Color(.95f, .76f, .48f));
        }

        private static void RefineGreenhouse(Transform root, Transform q, Mats m)
        {
            // The greenhouse was already the strongest middle deck in the screenshots. Preserve its
            // symmetric beds and add only service/drain cues so it remains clean rather than cluttered.
            Cube(q, "GreenhouseDrainL", new Vector3(0f, L.MainTop + .022f, -1.18f), new Vector3(L.CarLength - 1.4f, .024f, .08f), m.Steel, false);
            Cube(q, "GreenhouseDrainR", new Vector3(0f, L.MainTop + .022f, 1.18f), new Vector3(L.CarLength - 1.4f, .024f, .08f), m.Steel, false);
            foreach (float x in new[] { -5f, 0f, 5f })
                Cube(q, "GreenhouseServiceMarker", new Vector3(x, 2.55f, 4.36f), new Vector3(1.1f, .18f, .035f), m.Green, false);
        }

        // ---- Upper accommodation -------------------------------------------------------------

        private static void RefineUpperDeck(Transform root, Transform marker, int index, Mats m)
        {
            Transform upper = FindFirst(root, "UpperSleepingDeck");
            if (upper == null) return;

            Transform q = Group(marker, "UpperDeckQA", Vector3.zero);
            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            Material accent = AccentFor(index, m);

            // Warm light spilling from the cabins makes the currently black door openings legible and
            // visually separates private rooms from the public corridor.
            foreach (float u in new[] { -4.85f, 2.15f })
            {
                float x = s * u;
                foreach (float side in new[] { -1f, 1f })
                {
                    float z = side * 2.55f;
                    Point(q, "CabinWarmLight", new Vector3(x, 5.48f, z), .72f, 3.6f, new Color(1f, .66f, .39f));
                    Cube(q, "CabinNumberPlate", new Vector3(x - s * .38f, 4.62f, side * 1.27f), new Vector3(.34f, .20f, .035f), accent, false);
                    Cube(q, "CabinThreshold", new Vector3(x, L.UpperTop + .025f, side * 1.22f), new Vector3(.90f, .025f, .16f), accent, false);
                }
            }

            float vestibuleX = s * 6.70f;
            Point(q, "VestibuleLight", new Vector3(vestibuleX, 5.56f, 0f), 1.0f, 4.5f, new Color(.78f, .86f, .92f));
            Cube(q, "VestibuleCeilingPanel", new Vector3(vestibuleX, L.RoofBottom - .07f, 0f), new Vector3(2.25f, .08f, 1.35f), m.Frame, false);
            Cube(q, "VestibuleIdentityBand", new Vector3(s * 5.62f, 4.86f, 0f), new Vector3(.06f, .42f, 2.20f), accent, false);
        }

        // ---- Exterior train body -------------------------------------------------------------

        private static void RefineExterior(Transform root, Transform marker, int index, Mats m)
        {
            Transform q = Group(marker, "ExteriorQA", Vector3.zero);
            Material accent = AccentFor(index, m);

            // Roof chamfers break the tall rectangular silhouette seen in the side overview.
            GameObject roofL = Cube(q, "RoofChamferL", new Vector3(0f, L.RoofY - .04f, -4.28f), new Vector3(L.CarLength - .45f, .22f, .88f), m.Exterior, false);
            roofL.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            GameObject roofR = Cube(q, "RoofChamferR", new Vector3(0f, L.RoofY - .04f, 4.28f), new Vector3(L.CarLength - .45f, .22f, .88f), m.Exterior, false);
            roofR.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);

            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * (L.HalfW + .025f);
                Cube(q, "ExteriorRoleBand", new Vector3(0f, .73f, z), new Vector3(L.CarLength - .55f, .18f, .055f), accent, false);
                Cube(q, "ExteriorLowerSill", new Vector3(0f, -2.78f, z), new Vector3(L.CarLength - .65f, .34f, .08f), m.Frame, false);

                foreach (float bogieX in new[] { -6f, 6f })
                {
                    foreach (float dx in new[] { -.72f, .72f })
                        WheelDisc(q, new Vector3(bogieX + dx, -3.48f, side * 4.64f), side, m);
                    Cube(q, "BogieSideBeam", new Vector3(bogieX, -3.18f, side * 4.58f), new Vector3(2.45f, .26f, .16f), m.Frame, false);
                }
            }

            // End plates/couplers make the gaps read as connected railway cars rather than boxes.
            if (index > 0) Coupler(q, -L.HalfL - .26f, m);
            if (index < L.Cars.Length - 1) Coupler(q, L.HalfL + .26f, m);

            if (index == 0) BuildLocomotiveNose(q, m);
        }

        private static void BuildLocomotiveNose(Transform q, Mats m)
        {
            // Keep all new nose mass below the panoramic windshield bottom (1.10 m).
            Cube(q, "NoseLower", new Vector3(-9.58f, -.05f, 0f), new Vector3(1.16f, .68f, 7.70f), m.Exterior, false);
            Cube(q, "NoseMid", new Vector3(-9.28f, .50f, 0f), new Vector3(.62f, .42f, 7.92f), m.Frame, false);
            Cube(q, "NoseAccent", new Vector3(-9.62f, .34f, 0f), new Vector3(.10f, .18f, 7.76f), m.Hazard, false);
            Cube(q, "WindshieldBrow", new Vector3(-9.08f, 2.82f, 0f), new Vector3(.20f, .16f, 8.38f), m.Frame, false);
            foreach (float z in new[] { -3.18f, 3.18f })
                Cube(q, "NoseLampBezel", new Vector3(-9.92f, .54f, z), new Vector3(.12f, .48f, .64f), m.Steel, false);
        }

        private static void WheelDisc(Transform p, Vector3 pos, float side, Mats m)
        {
            GameObject wheel = Cylinder(p, "Wheel", pos, new Vector3(.54f, .18f, .54f), new Vector3(90f, 0f, 0f), m.Frame, false);
            GameObject rim = Cylinder(p, "WheelRim", pos + new Vector3(0f, 0f, side * .10f), new Vector3(.32f, .04f, .32f), new Vector3(90f, 0f, 0f), m.Steel, false);
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void Coupler(Transform p, float x, Mats m)
        {
            Cube(p, "CouplerStem", new Vector3(x, -2.85f, 0f), new Vector3(.68f, .20f, .26f), m.Frame, false);
            Cube(p, "CouplerHead", new Vector3(x + Mathf.Sign(x) * .28f, -2.85f, 0f), new Vector3(.28f, .46f, .58f), m.Steel, false);
        }

        // ---- Exterior world -----------------------------------------------------------------

        private static void RefineExteriorScene(Mats m)
        {
            Scene scene = File.Exists(ScenePath) ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single) : SceneManager.GetActiveScene();
            GameObject env = GameObject.Find("OUTSIDE_ENVIRONMENT");
            if (env == null) return;

            Transform near = FindFirst(env.transform, "NEAR_SCENERY");
            Transform far = FindFirst(env.transform, "DISTANT_SCENERY");
            if (near != null)
            {
                RemoveNamedChildrenRecursive(near, "PassingRock", "SnowDrift");
                Transform old = near.Find("QA_RockField");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                BuildRockField(near, m);
            }
            if (far != null)
            {
                RemoveNamedChildrenRecursive(far, "DistantMountain");
                Transform old = far.Find("QA_MountainRange");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                BuildMountainRange(far, m);
            }

            // Widen the visual rail gauge a little for the unusually wide three-deck body and extend sleepers.
            Transform[] rails = env.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Rail").ToArray();
            for (int i = 0; i < rails.Length; i++)
            {
                Vector3 p = rails[i].localPosition;
                p.z = (i % 2 == 0 ? -1f : 1f) * 3.15f;
                rails[i].localPosition = p;
            }
            foreach (Transform sleeper in env.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Sleeper"))
            {
                Vector3 s = sleeper.localScale;
                s.z = 7.65f;
                sleeper.localScale = s;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void BuildMountainRange(Transform far, Mats m)
        {
            Mesh mesh = GetOrCreateMountainMesh();
            System.Random rng = new System.Random(41017);
            Transform group = Group(far, "QA_MountainRange", Vector3.zero);
            for (int i = 0; i < 14; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float x = -82f + i * 13.2f;
                float z = side * (31f + (float)rng.NextDouble() * 13f);
                float sx = 11f + (float)rng.NextDouble() * 12f;
                float sy = 10f + (float)rng.NextDouble() * 12f;
                float sz = 9f + (float)rng.NextDouble() * 10f;

                Transform mountain = MeshObject(group, "Mountain", mesh, new Vector3(x, L.GroundY - .2f, z), new Vector3(sx, sy, sz), m.Rock);
                mountain.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 55f - 27f, 0f);
                Transform cap = MeshObject(mountain, "SnowCap", mesh, new Vector3(0f, .58f, 0f), new Vector3(.76f, .38f, .76f), m.Snow);
                cap.localRotation = Quaternion.Euler(0f, 14f, 0f);
            }
        }

        private static void BuildRockField(Transform near, Mats m)
        {
            Mesh mesh = GetOrCreateMountainMesh();
            System.Random rng = new System.Random(7721);
            Transform group = Group(near, "QA_RockField", Vector3.zero);
            for (int i = 0; i < 18; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float x = -58f + i * 7.2f;
                float z = side * (11f + (float)rng.NextDouble() * 7f);
                float sx = 1.8f + (float)rng.NextDouble() * 3.2f;
                float sy = .8f + (float)rng.NextDouble() * 1.6f;
                float sz = 1.6f + (float)rng.NextDouble() * 2.8f;
                Transform rock = MeshObject(group, "Rock", mesh, new Vector3(x, L.GroundY, z), new Vector3(sx, sy, sz), m.Rock);
                rock.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 180f, 0f);
                MeshObject(rock, "SnowCap", mesh, new Vector3(0f, .50f, 0f), new Vector3(.82f, .34f, .82f), m.Snow);
            }

            for (int i = 0; i < 18; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                GameObject drift = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                drift.name = "SoftSnowDrift";
                drift.transform.SetParent(group, false);
                drift.transform.localPosition = new Vector3(-56f + i * 7f, L.GroundY + .18f, side * (6.3f + (i % 3) * .65f));
                drift.transform.localScale = new Vector3(4.2f + (i % 4), .42f + (i % 2) * .12f, 1.5f + (i % 3) * .32f);
                Renderer r = drift.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = m.Snow;
                Collider c = drift.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
        }

        private static Mesh GetOrCreateMountainMesh()
        {
            EnsureFolder("Assets/Art/Meshes");
            EnsureFolder(MeshRoot);
            string path = MeshRoot + "/FT_LowPolyMountain.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            const int sides = 8;
            var vertices = new List<Vector3>();
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                float radius = 1f + Mathf.Sin(i * 2.31f) * .08f;
                vertices.Add(new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
            }
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides + .12f;
                float radius = .52f + Mathf.Cos(i * 1.77f) * .06f;
                vertices.Add(new Vector3(Mathf.Cos(a) * radius, .48f, Mathf.Sin(a) * radius));
            }
            vertices.Add(new Vector3(.08f, 1f, -.04f));
            int apex = vertices.Count - 1;

            var triangles = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                int lowerA = i, lowerB = next, midA = sides + i, midB = sides + next;
                triangles.Add(lowerA); triangles.Add(midA); triangles.Add(lowerB);
                triangles.Add(lowerB); triangles.Add(midA); triangles.Add(midB);
                triangles.Add(midA); triangles.Add(apex); triangles.Add(midB);
            }

            Mesh mesh = new Mesh { name = "FT_LowPolyMountain" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        // ---- Helpers ------------------------------------------------------------------------

        private static Mats LoadMaterials() => new Mats
        {
            Floor = Mat("FT_PRO_Floor", new Color(.08f, .09f, .10f)),
            Wall = Mat("FT_PRO_Wall", new Color(.16f, .18f, .19f)),
            Frame = Mat("FT_PRO_Frame", new Color(.04f, .05f, .06f)),
            Steel = Mat("FT_PRO_BrushedSteel", new Color(.62f, .66f, .68f)),
            Fabric = Mat("FT_PRO_Fabric", new Color(.10f, .18f, .22f)),
            Hazard = Mat("FT_PRO_Hazard", new Color(.92f, .47f, .055f)),
            Generator = Mat("FT_PRO_Generator", new Color(.34f, .105f, .028f)),
            Workshop = Mat("FT_PRO_Workshop", new Color(.34f, .245f, .075f)),
            Green = Mat("FT_PRO_Greenhouse", new Color(.055f, .28f, .13f)),
            Screen = Mat("FT_PRO_Screen", new Color(.03f, .45f, .60f)),
            Warm = Mat("FT_PRO_WarmLight", new Color(.90f, .55f, .18f)),
            Exterior = Mat("FT_PRO_Exterior", new Color(.075f, .095f, .115f)),
            Snow = FindAnyMaterial("Train_Snow", Color.white),
            Rock = FindAnyMaterial("Train_FarRock", new Color(.12f, .14f, .17f)),
        };

        private static Material AccentFor(int index, Mats m)
        {
            switch (index)
            {
                case 0: return m.Hazard;
                case 1: return m.Generator;
                case 2: return m.Fabric;
                case 3: return m.Workshop;
                default: return m.Green;
            }
        }

        private static Material Mat(string name, Color fallback)
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/" + name + ".mat");
            return m != null ? m : TemporaryMaterial(name + "_Fallback", fallback);
        }

        private static Material FindAnyMaterial(string name, Color fallback)
        {
            string[] ids = AssetDatabase.FindAssets(name + " t:Material");
            if (ids.Length > 0)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(ids[0]));
                if (m != null) return m;
            }
            return TemporaryMaterial(name + "_Fallback", fallback);
        }

        private static Material TemporaryMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { name = name, color = color };
        }

        private static void MoveLocal(Transform root, string name, float? x, float? z, float? y)
        {
            Transform t = FindFirst(root, name);
            if (t == null) return;
            Vector3 p = t.localPosition;
            if (x.HasValue) p.x = x.Value;
            if (y.HasValue) p.y = y.Value;
            if (z.HasValue) p.z = z.Value;
            t.localPosition = p;
        }

        private static void Chair(Transform p, string name, Vector3 pos, float yaw, Mats m)
        {
            Transform c = Group(p, name, pos);
            c.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Cube(c, "Base", new Vector3(0f, .26f, 0f), new Vector3(.48f, .42f, .48f), m.Frame, false);
            Cube(c, "Seat", new Vector3(0f, .54f, 0f), new Vector3(.64f, .14f, .60f), m.Fabric, false);
            Cube(c, "Back", new Vector3(-.27f, 1.00f, 0f), new Vector3(.12f, .82f, .60f), m.Fabric, false);
        }

        private static Transform MeshObject(Transform parent, string name, Mesh mesh, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            return go.transform;
        }

        private static void RemoveNamedChildrenRecursive(Transform root, params string[] names)
        {
            HashSet<string> set = new HashSet<string>(names);
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                Transform t = all[i];
                if (t != null && t != root && set.Contains(t.name)) UnityEngine.Object.DestroyImmediate(t.gameObject);
            }
        }

        private static Transform Group(Transform parent, string name, Vector3 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
        }

        private static GameObject Cylinder(Transform parent, string name, Vector3 pos, Vector3 size, Vector3 euler, Material mat, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = new Vector3(size.x, size.y * .5f, size.z);
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
        }

        private static void Point(Transform parent, string name, Vector3 pos, float intensity, float range, Color color)
        {
            Transform t = Group(parent, name, pos);
            Light l = t.gameObject.AddComponent<Light>();
            l.type = LightType.Point;
            l.intensity = intensity;
            l.range = range;
            l.color = color;
            l.shadows = LightShadows.None;
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

        private static IEnumerable<Transform> FindAll(Transform root, string name) =>
            root.GetComponentsInChildren<Transform>(true).Where(t => t != null && t.name == name).ToArray();

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            if (!string.IsNullOrEmpty(parent)) AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
