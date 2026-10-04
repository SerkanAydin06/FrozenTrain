#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Converts the five-car prototype from "walk through each room" circulation into a scalable
    /// horizontal-building layout: lower = infrastructure, main = public Main Street, upper = private
    /// residential. This is an authored final architecture pass over generated prefabs; it never changes
    /// survival bindings and is deliberately idempotent.
    /// </summary>
    internal static class FrozenTrainMainStreetArchitecture
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MarkerName = "__MainStreetArchitectureV1";
        private const float StreetHalf = 1.38f;
        private const float PartitionZ = 1.62f;
        private const float StreetFloorY = .126f;
        private const float MainCeilingY = 3.02f;
        private const float UpperFloor = 3.24f;

        private sealed class Mats
        {
            public Material Floor;
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material Glass;
            public Material Hazard;
            public Material Fabric;
            public Material Generator;
            public Material Workshop;
            public Material Green;
            public Material Screen;
            public Material Warm;
        }

        public static void Apply()
        {
            Mats mats = LoadMaterials();
            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform old = root.transform.Find(MarkerName);
                    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                    Transform marker = Group(root.transform, MarkerName);

                    ClearOldAisleGuides(root.transform);
                    RezoneMainDeck(root.transform, marker, i, mats);
                    ConvertUpperToResidential(root.transform, marker, i, mats);
                    AddDeckIdentity(marker, i, mats);

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ClearOldAisleGuides(Transform root)
        {
            foreach (string n in new[] { "MainAisleGuide", "MainAisleEdgeL", "MainAisleEdgeR" })
                foreach (Transform t in FindAll(root, n).ToArray())
                    if (t != null) UnityEngine.Object.DestroyImmediate(t.gameObject);
        }

        private static void RezoneMainDeck(Transform root, Transform marker, int index, Mats m)
        {
            Transform street = Group(marker, "MainStreet");
            Material accent = Accent(index, m);

            // One continuous public spine. 2.76 m is wide enough for two-way movement and future NPC traffic.
            Cube(street, "StreetRunner", new Vector3(0f, StreetFloorY, 0f),
                new Vector3(L.CarLength - .38f, .032f, StreetHalf * 2f), m.Floor, false);
            Cube(street, "StreetEdgeL", new Vector3(0f, StreetFloorY + .018f, -StreetHalf),
                new Vector3(L.CarLength - .45f, .026f, .055f), accent, false);
            Cube(street, "StreetEdgeR", new Vector3(0f, StreetFloorY + .018f, StreetHalf),
                new Vector3(L.CarLength - .45f, .026f, .055f), accent, false);

            // Repeated ceiling spine makes all cars read as one continuous civic route.
            Cube(street, "StreetCeilingSpine", new Vector3(0f, MainCeilingY - .10f, 0f),
                new Vector3(L.CarLength - .45f, .14f, .34f), m.Frame, false);
            foreach (float x in new[] { -6.1f, -2.05f, 2.05f, 6.1f })
            {
                Cube(street, "StreetLight", new Vector3(x, MainCeilingY - .16f, 0f),
                    new Vector3(2.25f, .055f, .23f), m.Warm, false);
                AddPoint(street, "StreetFill", new Vector3(x, 2.60f, 0f), .82f, 4.8f, new Color(.90f, .82f, .68f));
            }

            BuildStreetVestibules(street, index, m);

            switch (index)
            {
                case 0: BuildLocomotiveFrontage(root, street, m); break;
                case 1: BuildGeneratorFrontage(root, street, m); break;
                case 2: BuildLivingFrontage(root, street, m); break;
                case 3: BuildWorkshopFrontage(root, street, m); break;
                case 4: BuildGreenhouseFrontage(root, street, m); break;
            }
        }

        private static void BuildStreetVestibules(Transform street, int index, Mats m)
        {
            Material accent = Accent(index, m);
            foreach (float x in new[] { -8.36f, 8.36f })
            {
                // Funnel the 2.76 m street gently into the current connector opening instead of ending abruptly.
                Cube(street, "VestibuleHeader", new Vector3(x, 2.64f, 0f), new Vector3(.18f, .16f, 3.25f), m.Frame, false);
                Cube(street, "VestibuleBand", new Vector3(x - Mathf.Sign(x) * .11f, 2.32f, 0f), new Vector3(.05f, .32f, 2.64f), accent, false);
                foreach (float side in new[] { -1f, 1f })
                    Cube(street, "VestibuleGuard", new Vector3(x, .54f, side * 1.50f), new Vector3(.48f, .78f, .07f), m.Steel, false);
            }
        }

        private static void BuildLocomotiveFrontage(Transform root, Transform street, Mats m)
        {
            // Main Street terminates at a proper cab lobby; the driving console remains a restricted room.
            MoveToZ(root, "EquipmentCabinet", 3.96f);
            Transform terminal = FindFirst(root, "ManagementTerminal");
            if (terminal != null) terminal.localPosition = new Vector3(4.95f, terminal.localPosition.y, 3.82f);

            const float x = -4.88f;
            BuildTransversePortal(street, "CabSecurityPortal", x, m.Frame, m.Glass, 1.25f);
            Cube(street, "CabLobbyBand", new Vector3(-4.45f, StreetFloorY + .018f, 0f), new Vector3(.65f, .025f, 2.55f), m.Hazard, false);
            AddPoint(street, "CabLobbyLight", new Vector3(-4.20f, 2.45f, 0f), 1.0f, 4.8f, new Color(.70f, .82f, .95f));
        }

        private static void BuildGeneratorFrontage(Transform root, Transform street, Mats m)
        {
            MoveToZ(root, "OperatorDesk", -3.88f);
            MoveToZ(root, "GeneratorConsole", -4.05f);
            MoveToZ(root, "MonitoringRack", 3.96f);
            MoveToZ(root, "ControlTable", -3.86f);

            BuildGlazedFrontage(street, -1f, m.Generator, m, new[] { -5.2f, .5f, 5.4f });
            BuildGlazedFrontage(street, 1f, m.Generator, m, new[] { -4.7f, 1.2f, 5.8f });
            Cube(street, "GeneratorObservationRail", new Vector3(0f, 1.02f, PartitionZ),
                new Vector3(15.6f, .08f, .09f), m.Steel, false);
        }

        private static void BuildLivingFrontage(Transform root, Transform street, Mats m)
        {
            // Living car is the one intentionally open frontage: seating becomes cafe/lounge bays off the street.
            foreach (Transform bay in FindAll(root, "SeatingBay"))
            {
                Vector3 p = bay.localPosition;
                p.z = Mathf.Sign(Mathf.Abs(p.z) < .01f ? 1f : p.z) * 3.42f;
                bay.localPosition = p;
            }
            MoveToZ(root, "GalleyCounter", -4.00f);
            MoveToZ(root, "GalleyTop", -4.00f);
            Transform med = FindFirst(root, "MedicalCabinet");
            if (med != null) med.localPosition = new Vector3(3.85f, med.localPosition.y, 4.04f);

            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * PartitionZ;
                foreach (float x in new[] { -6.5f, -1.0f, 4.6f })
                {
                    Cube(street, "LivingPortalPost", new Vector3(x, 1.30f, z), new Vector3(.10f, 2.35f, .10f), m.Frame, false);
                    Cube(street, "LivingPortalLamp", new Vector3(x, 2.60f, z), new Vector3(.58f, .08f, .12f), m.Warm, false);
                }
            }
            Cube(street, "LivingCommunityStrip", new Vector3(.3f, StreetFloorY + .022f, 0f),
                new Vector3(7.3f, .022f, 2.40f), m.Fabric, false);
        }

        private static void BuildWorkshopFrontage(Transform root, Transform street, Mats m)
        {
            foreach (Transform bench in FindAll(root, "Workbench"))
            {
                Vector3 p = bench.localPosition;
                p.z = Mathf.Sign(Mathf.Abs(p.z) < .01f ? 1f : p.z) * 3.82f;
                bench.localPosition = p;
            }
            MoveToZ(root, "ToolCabinet", 4.00f);
            Transform machine = FindFirst(root, "MachineStation");
            if (machine != null) machine.localPosition = new Vector3(5.45f, machine.localPosition.y, 3.75f);
            Transform screen = FindFirst(root, "MachineScreen");
            if (screen != null) screen.localPosition = new Vector3(5.45f, screen.localPosition.y, 3.00f);

            BuildGlazedFrontage(street, -1f, m.Workshop, m, new[] { -5.0f, 1.2f, 6.0f });
            BuildGlazedFrontage(street, 1f, m.Workshop, m, new[] { -5.8f, -.2f, 5.4f });

            // Safety kick plate stops carts/tools rolling into the public route but keeps the workshop visible.
            foreach (float side in new[] { -1f, 1f })
                Cube(street, "WorkshopKick", new Vector3(0f, .30f, side * PartitionZ),
                    new Vector3(15.7f, .52f, .08f), m.Frame, false);
        }

        private static void BuildGreenhouseFrontage(Transform root, Transform street, Mats m)
        {
            foreach (Transform bed in FindAll(root, "HydroponicBed"))
            {
                Vector3 p = bed.localPosition;
                p.z = Mathf.Sign(Mathf.Abs(p.z) < .01f ? 1f : p.z) * 3.00f;
                bed.localPosition = p;
            }
            MoveToZ(root, "ClimatePanel", -4.00f);
            MoveToZ(root, "ClimateScreen", -3.72f);

            BuildGlazedFrontage(street, -1f, m.Green, m, new[] { -5.8f, 0f, 5.8f });
            BuildGlazedFrontage(street, 1f, m.Green, m, new[] { -5.8f, 0f, 5.8f });
            foreach (float x in new[] { -6f, -2f, 2f, 6f })
                Cube(street, "GreenhouseViewLight", new Vector3(x, 2.58f, 0f), new Vector3(1.65f, .06f, .18f), m.Green, false);
        }

        private static void BuildGlazedFrontage(Transform parent, float side, Material accent, Mats m, float[] accessCenters)
        {
            float z = side * PartitionZ;
            const float xMin = -7.95f, xMax = 7.95f, openingHalf = .72f;
            List<float> cuts = new List<float> { xMin, xMax };
            foreach (float c in accessCenters)
            {
                cuts.Add(Mathf.Clamp(c - openingHalf, xMin, xMax));
                cuts.Add(Mathf.Clamp(c + openingHalf, xMin, xMax));
            }
            cuts = cuts.Distinct().OrderBy(v => v).ToList();

            // Frame posts remain at every opening edge; glass only fills intervals that are not access doors.
            foreach (float c in accessCenters)
            {
                Cube(parent, "FrontageDoorPost", new Vector3(c - openingHalf, 1.35f, z), new Vector3(.09f, 2.42f, .09f), m.Frame, false);
                Cube(parent, "FrontageDoorPost", new Vector3(c + openingHalf, 1.35f, z), new Vector3(.09f, 2.42f, .09f), m.Frame, false);
                Cube(parent, "FrontageDoorHeader", new Vector3(c, 2.56f, z), new Vector3(openingHalf * 2f, .10f, .11f), accent, false);
            }

            float cursor = xMin;
            foreach (float c in accessCenters.OrderBy(v => v))
            {
                float a = Mathf.Max(cursor, c - openingHalf);
                if (a > cursor + .08f) GlassSegment(parent, cursor, a, z, side, m);
                cursor = Mathf.Max(cursor, c + openingHalf);
            }
            if (cursor < xMax - .08f) GlassSegment(parent, cursor, xMax, z, side, m);
        }

        private static void GlassSegment(Transform p, float x0, float x1, float z, float side, Mats m)
        {
            float x = (x0 + x1) * .5f, w = x1 - x0;
            Cube(p, "FrontageBottomRail", new Vector3(x, .56f, z), new Vector3(w, .09f, .10f), m.Frame, false);
            Cube(p, "FrontageTopRail", new Vector3(x, 2.56f, z), new Vector3(w, .09f, .10f), m.Frame, false);
            Cube(p, "SafetyGlass", new Vector3(x, 1.55f, z + side * .015f), new Vector3(w - .08f, 1.82f, .025f), m.Glass, false);
        }

        private static void BuildTransversePortal(Transform p, string name, float x, Material frame, Material glass, float halfWidth)
        {
            Transform g = Group(p, name);
            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * (halfWidth + (L.HalfW - halfWidth) * .5f);
                float width = L.HalfW - halfWidth;
                Cube(g, "CabBulkhead", new Vector3(x, 1.52f, z), new Vector3(.10f, 2.80f, width), frame, false);
            }
            Cube(g, "CabGlassDoor", new Vector3(x + .015f, 1.50f, 0f), new Vector3(.035f, 2.35f, halfWidth * 2f), glass, false);
            Cube(g, "CabDoorHeader", new Vector3(x, 2.78f, 0f), new Vector3(.12f, .18f, halfWidth * 2f), frame, false);
        }

        private static void ConvertUpperToResidential(Transform root, Transform marker, int index, Mats m)
        {
            Transform upper = FindFirst(root, "UpperSleepingDeck");
            if (upper == null) return;
            Transform residential = Group(marker, "ResidentialDeck");
            Material accent = Accent(index, m);

            // Upper deck is no longer a through-route. Privacy bulkheads close both inter-car end openings.
            foreach (float x in new[] { -8.86f, 8.86f })
            {
                Cube(residential, "ResidentialEndBulkhead", new Vector3(x, 4.72f, 0f),
                    new Vector3(.12f, 2.82f, 8.64f), m.Wall, true);
                Cube(residential, "ResidentialEndBand", new Vector3(x - Mathf.Sign(x) * .07f, 4.95f, 0f),
                    new Vector3(.04f, .34f, 2.35f), accent, false);
            }

            // Reframe the former transit corridor as a quiet local hall. It remains useful for reaching rooms
            // from the two ladders but does not connect to the neighbouring car.
            Cube(residential, "ResidentialHallRunner", new Vector3(0f, UpperFloor + .018f, 0f),
                new Vector3(16.9f, .028f, 1.84f), m.Fabric, false);
            foreach (float x in new[] { -5.6f, 0f, 5.6f })
            {
                Cube(residential, "ResidentialHallLight", new Vector3(x, 6.02f, 0f), new Vector3(1.65f, .055f, .20f), m.Warm, false);
                AddPoint(residential, "ResidentialFill", new Vector3(x, 5.52f, 0f), .64f, 3.8f, new Color(1f, .72f, .48f));
            }

            // Quiet-zone threshold around each ladder vestibule.
            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            float landingX = s * 6.15f;
            Cube(residential, "ResidentialLanding", new Vector3(landingX, UpperFloor + .022f, 0f),
                new Vector3(2.15f, .025f, 2.15f), m.Floor, false);
            Cube(residential, "ResidentialLandingBand", new Vector3(landingX - s * .95f, UpperFloor + .035f, 0f),
                new Vector3(.08f, .025f, 2.05f), accent, false);
        }

        private static void AddDeckIdentity(Transform marker, int index, Mats m)
        {
            Material accent = Accent(index, m);
            Transform signs = Group(marker, "Wayfinding");
            foreach (float x in new[] { -6.9f, 6.9f })
            {
                Cube(signs, "MainStreetMarker", new Vector3(x, 2.70f, 0f), new Vector3(.72f, .22f, .36f), accent, false);
                Cube(signs, "MainStreetArrow", new Vector3(x - Mathf.Sign(x) * .46f, 2.70f, 0f), new Vector3(.28f, .09f, .72f), m.Steel, false);
            }
        }

        private static Mats LoadMaterials()
        {
            return new Mats
            {
                Floor = Mat("FT_PRO_Floor", new Color(.08f, .09f, .10f)),
                Wall = Mat("FT_PRO_Wall", new Color(.16f, .18f, .19f)),
                Frame = Mat("FT_PRO_Frame", new Color(.04f, .05f, .06f)),
                Steel = Mat("FT_PRO_BrushedSteel", new Color(.62f, .66f, .68f)),
                Glass = FindGlass(),
                Hazard = Mat("FT_PRO_Hazard", new Color(.92f, .47f, .055f)),
                Fabric = Mat("FT_PRO_Fabric", new Color(.10f, .18f, .22f)),
                Generator = Mat("FT_PRO_Generator", new Color(.34f, .105f, .028f)),
                Workshop = Mat("FT_PRO_Workshop", new Color(.34f, .245f, .075f)),
                Green = Mat("FT_PRO_Greenhouse", new Color(.055f, .28f, .13f)),
                Screen = Mat("FT_PRO_Screen", new Color(.03f, .45f, .60f)),
                Warm = Mat("FT_PRO_WarmLight", new Color(.90f, .55f, .18f)),
            };
        }

        private static Material Accent(int index, Mats m)
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
            if (m != null) return m;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { name = name + "_Fallback", color = fallback };
        }

        private static Material FindGlass()
        {
            string[] ids = AssetDatabase.FindAssets("Glass t:Material");
            if (ids.Length > 0)
            {
                Material found = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(ids[0]));
                if (found != null) return found;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material m = new Material(shader) { name = "MainStreetGlass_Fallback", color = new Color(.20f, .36f, .42f, .28f) };
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", new Color(.20f, .36f, .42f, .28f));
            m.renderQueue = 3000;
            return m;
        }

        private static void MoveToZ(Transform root, string name, float z)
        {
            foreach (Transform t in FindAll(root, name))
            {
                Vector3 p = t.localPosition;
                p.z = z;
                t.localPosition = p;
            }
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
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

        private static void AddPoint(Transform parent, string name, Vector3 pos, float intensity, float range, Color color)
        {
            Transform t = Group(parent, name);
            t.localPosition = pos;
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

        private static IEnumerable<Transform> FindAll(Transform root, string name)
        {
            if (root == null) yield break;
            if (root.name == name) yield return root;
            foreach (Transform child in root)
                foreach (Transform found in FindAll(child, name))
                    yield return found;
        }
    }
}
#endif
