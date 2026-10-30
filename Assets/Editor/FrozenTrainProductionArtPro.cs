#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Production visual pass for all five FrozenTrain cars. The canonical layout/builder still owns
    /// architecture, circulation and gameplay bindings; this pass replaces the greybox look with a
    /// deterministic, free-only material/detail library and preserves stable interaction roots.
    /// </summary>
    internal static class FrozenTrainProductionArtPro
    {
        private const string ArtRoot = "Assets/Art";
        private const string TextureRoot = ArtRoot + "/Textures/Generated/TrainProduction";
        private const string MaterialRoot = ArtRoot + "/Materials/TrainProduction";
        private const string TrainPrefabRoot = "Assets/Prefabs/Train";
        private const int TextureSize = 512;

        private sealed class Mats
        {
            public Material Floor;
            public Material Wall;
            public Material Frame;
            public Material Steel;
            public Material BrushedSteel;
            public Material Rubber;
            public Material Fabric;
            public Material Bedding;
            public Material Table;
            public Material Generator;
            public Material Workshop;
            public Material Green;
            public Material WhiteEnamel;
            public Material Hazard;
            public Material Copper;
            public Material Screen;
            public Material WarmLight;
            public Material Plant;
            public Material Exterior;
        }

        public static void Apply()
        {
            EnsureFolders();
            Mats mats = BuildMaterials();

            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = TrainPrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                UpgradeCar(path, i, mats);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ArtRoot);
            EnsureFolder(ArtRoot + "/Textures");
            EnsureFolder(ArtRoot + "/Textures/Generated");
            EnsureFolder(TextureRoot);
            EnsureFolder(ArtRoot + "/Materials");
            EnsureFolder(MaterialRoot);
        }

        private static Mats BuildMaterials()
        {
            string floorA = TextureRoot + "/FT_PRO_Floor_Albedo.png";
            string floorN = TextureRoot + "/FT_PRO_Floor_Normal.png";
            string wallA = TextureRoot + "/FT_PRO_Wall_Albedo.png";
            string wallN = TextureRoot + "/FT_PRO_Wall_Normal.png";
            string steelA = TextureRoot + "/FT_PRO_Steel_Albedo.png";
            string steelN = TextureRoot + "/FT_PRO_Steel_Normal.png";
            string fabricA = TextureRoot + "/FT_PRO_Fabric_Albedo.png";
            string fabricN = TextureRoot + "/FT_PRO_Fabric_Normal.png";
            string beddingA = TextureRoot + "/FT_PRO_Bedding_Albedo.png";
            string tableA = TextureRoot + "/FT_PRO_Table_Albedo.png";
            string tableN = TextureRoot + "/FT_PRO_Table_Normal.png";

            EnsureTexture(floorA, false, FloorAlbedo);
            EnsureTexture(floorN, true, FloorNormal);
            EnsureTexture(wallA, false, WallAlbedo);
            EnsureTexture(wallN, true, WallNormal);
            EnsureTexture(steelA, false, SteelAlbedo);
            EnsureTexture(steelN, true, SteelNormal);
            EnsureTexture(fabricA, false, FabricAlbedo);
            EnsureTexture(fabricN, true, FabricNormal);
            EnsureTexture(beddingA, false, BeddingAlbedo);
            EnsureTexture(tableA, false, TableAlbedo);
            EnsureTexture(tableN, true, TableNormal);

            return new Mats
            {
                Floor = Textured("FT_PRO_Floor", floorA, floorN, .24f, .28f, new Color(.72f, .76f, .80f), 3.5f),
                Wall = Textured("FT_PRO_Wall", wallA, wallN, .20f, .34f, new Color(.76f, .79f, .80f), 2.0f),
                Frame = Solid("FT_PRO_Frame", new Color(.035f, .045f, .055f), .50f, .42f),
                Steel = Textured("FT_PRO_Steel", steelA, steelN, .76f, .34f, new Color(.72f, .76f, .78f), 2.2f),
                BrushedSteel = Textured("FT_PRO_BrushedSteel", steelA, steelN, .90f, .58f, new Color(.86f, .88f, .90f), 1.2f),
                Rubber = Solid("FT_PRO_Rubber", new Color(.022f, .026f, .031f), .02f, .16f),
                Fabric = Textured("FT_PRO_Fabric", fabricA, fabricN, .02f, .23f, new Color(.58f, .69f, .76f), 2.4f),
                Bedding = Textured("FT_PRO_Bedding", beddingA, fabricN, 0f, .18f, new Color(.90f, .92f, .93f), 2.0f),
                Table = Textured("FT_PRO_Table", tableA, tableN, .08f, .38f, new Color(.75f, .73f, .68f), 1.6f),
                Generator = Solid("FT_PRO_Generator", new Color(.34f, .105f, .028f), .48f, .40f),
                Workshop = Solid("FT_PRO_Workshop", new Color(.34f, .245f, .075f), .30f, .34f),
                Green = Solid("FT_PRO_Greenhouse", new Color(.055f, .28f, .13f), .24f, .32f),
                WhiteEnamel = Solid("FT_PRO_WhiteEnamel", new Color(.70f, .75f, .75f), .18f, .42f),
                Hazard = Solid("FT_PRO_Hazard", new Color(.92f, .47f, .055f), .18f, .36f),
                Copper = Solid("FT_PRO_Copper", new Color(.40f, .18f, .075f), .78f, .48f),
                Screen = Emissive("FT_PRO_Screen", new Color(.025f, .11f, .14f), new Color(.08f, .74f, .92f) * 2.8f),
                WarmLight = Emissive("FT_PRO_WarmLight", new Color(.35f, .24f, .10f), new Color(1.0f, .55f, .18f) * 2.2f),
                Plant = Solid("FT_PRO_Plant", new Color(.035f, .32f, .085f), .02f, .24f),
                Exterior = Solid("FT_PRO_Exterior", new Color(.075f, .095f, .115f), .48f, .30f),
            };
        }

        private static void UpgradeCar(string path, int index, Mats m)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                Debug.LogWarning("[ProductionArt] Missing train prefab: " + path);
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                RemoveMarker(root.transform, "__ProductionArtV2");
                Transform marker = NewGroup(root.transform, "__ProductionArtV2", Vector3.zero, Quaternion.identity);

                UpgradeShell(root.transform, marker, index, m);
                UpgradeLighting(root.transform, marker, index, m);
                UpgradeTechnicalDeck(root.transform, marker, index, m);
                UpgradeUpperDeck(root.transform, marker, index, m);
                UpgradeAccess(root.transform, marker, index, m);
                UpgradeExterior(root.transform, marker, index, m);

                switch (index)
                {
                    case 0: UpgradeLocomotive(root.transform, marker, m); break;
                    case 1: UpgradeGenerator(root.transform, marker, m); break;
                    case 2: UpgradeLiving(root.transform, marker, m); break;
                    case 3: UpgradeWorkshop(root.transform, marker, m); break;
                    case 4: UpgradeGreenhouse(root.transform, marker, m); break;
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // ---- Common train skin ---------------------------------------------------------------

        private static void UpgradeShell(Transform root, Transform marker, int index, Mats m)
        {
            Transform shell = FindFirst(root, "Shell");
            if (shell == null) return;

            foreach (Renderer r in shell.GetComponentsInChildren<Renderer>(true))
            {
                string n = r.name;
                if (n.Contains("WindowGlass") || n.Contains("PanoramicWindshield")) continue;
                if (n.Contains("Floor")) r.sharedMaterial = m.Floor;
                else if (n.Contains("Underframe") || n.Contains("Bogie")) r.sharedMaterial = m.Frame;
                else if (n.Contains("Frame") || n.Contains("Lintel")) r.sharedMaterial = m.Frame;
                else r.sharedMaterial = m.Wall;
            }

            Transform skin = NewGroup(marker, "InteriorArchitecture", Vector3.zero, Quaternion.identity);
            foreach (float z in new[] { -L.HalfW + .18f, L.HalfW - .18f })
            {
                Cube(skin, "MainKickRail", new Vector3(0f, .30f, z), new Vector3(L.CarLength - .7f, .12f, .09f), m.BrushedSteel, false);
                Cube(skin, "UpperKickRail", new Vector3(0f, L.UpperTop + .22f, z), new Vector3(L.CarLength - .7f, .10f, .08f), m.Frame, false);
                Cube(skin, "LowerServiceRail", new Vector3(0f, L.LowerTop + .30f, z), new Vector3(L.CarLength - .8f, .10f, .10f), m.Hazard, false);
            }

            // Structural ribs break up the long flat walls without narrowing circulation.
            foreach (float x in new[] { -6.7f, -3.35f, 0f, 3.35f, 6.7f })
            {
                foreach (float z in new[] { -L.HalfW + .12f, L.HalfW - .12f })
                {
                    Cube(skin, "WallRib", new Vector3(x, 1.58f, z), new Vector3(.10f, 2.70f, .12f), m.Frame, false);
                    Cube(skin, "UpperWallRib", new Vector3(x, 4.72f, z), new Vector3(.10f, 2.65f, .12f), m.Frame, false);
                }
            }

            // Ceiling HVAC panels and emergency strips.
            foreach (float x in new[] { -5.7f, 0f, 5.7f })
            {
                // Flush-mounted: underside of the upper deck is the main-deck ceiling.
                float ventY = L.UpperY - L.UpperFloorT * .5f - .03f;
                Cube(skin, "MainCeilingVent", new Vector3(x, ventY, 2.25f), new Vector3(2.0f, .06f, .55f), m.Frame, false);
                Cube(skin, "MainCeilingVent", new Vector3(x, ventY, -2.25f), new Vector3(2.0f, .06f, .55f), m.Frame, false);
            }
            Cube(skin, "UpperEmergencyStripL", new Vector3(0f, L.UpperTop + .035f, -L.CorridorHalf + .08f), new Vector3(L.CarLength - .7f, .02f, .05f), m.Hazard, false);
            Cube(skin, "UpperEmergencyStripR", new Vector3(0f, L.UpperTop + .035f, L.CorridorHalf - .08f), new Vector3(L.CarLength - .7f, .02f, .05f), m.Hazard, false);
        }

        private static void UpgradeLighting(Transform root, Transform marker, int index, Mats m)
        {
            Transform lighting = FindFirst(root, "Lighting");
            if (lighting == null) return;

            foreach (Light light in lighting.GetComponentsInChildren<Light>(true))
            {
                light.shadows = LightShadows.None;
                light.range = Mathf.Max(light.range, 8.5f);
                light.intensity *= 1.55f;
            }

            foreach (Renderer r in lighting.GetComponentsInChildren<Renderer>(true))
                if (r.name.Contains("LightStrip")) r.sharedMaterial = m.WarmLight;

            Transform extra = NewGroup(marker, "ProductionLighting", Vector3.zero, Quaternion.identity);
            Color mainColor = L.Cars[index].MainLightColor;
            foreach (float x in new[] { -5.6f, 5.6f })
            {
                AddPoint(extra, "MainFill", new Vector3(x, 2.62f, 0f), 1.10f, 6.6f, mainColor);
                AddPoint(extra, "LowerFill", new Vector3(x, -.58f, 0f), .86f, 6.2f, new Color(.60f, .72f, .82f));
                AddPoint(extra, "UpperFill", new Vector3(x, 5.55f, 0f), .82f, 6.0f, new Color(.86f, .72f, .56f));
            }
        }

        private static void UpgradeTechnicalDeck(Transform root, Transform marker, int index, Mats m)
        {
            Transform lower = FindFirst(root, "LowerTechnicalDeck");
            if (lower == null) return;

            foreach (Renderer r in lower.GetComponentsInChildren<Renderer>(true))
            {
                string n = r.name;
                if (n.Contains("Power") || n.Contains("Cable") || n.Contains("Frame") || n.Contains("Body")) r.sharedMaterial = m.Frame;
                else if (n.Contains("Heating") || n.Contains("Fuel")) r.sharedMaterial = m.Hazard;
                else r.sharedMaterial = m.Steel;
            }

            Transform tech = NewGroup(marker, "TechnicalDeckDetails", Vector3.zero, Quaternion.identity);
            // Repeated pipe brackets and service labels give the lower deck a coherent engineering language.
            foreach (float x in new[] { -6f, -3f, 0f, 3f, 6f })
            {
                Cube(tech, "PipeBracketL", new Vector3(x, -1.40f, -4.11f), new Vector3(.12f, 1.18f, .10f), m.Frame, false);
                Cube(tech, "CableBracketR", new Vector3(x, -1.02f, 4.11f), new Vector3(.12f, .72f, .10f), m.Frame, false);
            }

            // Valves: compact disc + stem, positioned clear of the walking aisle.
            foreach (float x in new[] { -4.5f, 1.5f, 5.0f })
            {
                Cylinder(tech, "ServiceValve", new Vector3(x, -1.04f, -3.92f), new Vector3(.34f, .07f, .34f), new Vector3(90f, 0f, 0f), m.Hazard, false);
                Cylinder(tech, "ValveStem", new Vector3(x, -1.04f, -3.78f), new Vector3(.055f, .28f, .055f), new Vector3(90f, 0f, 0f), m.Steel, false);
            }

            // Floor hazard bars around machinery zones, never across the centre aisle.
            Cube(tech, "TechHazardL", new Vector3(0f, L.LowerTop + .025f, -2.35f), new Vector3(L.CarLength - 1.3f, .02f, .06f), m.Hazard, false);
            Cube(tech, "TechHazardR", new Vector3(0f, L.LowerTop + .025f, 2.35f), new Vector3(L.CarLength - 1.3f, .02f, .06f), m.Hazard, false);
        }

        private static void UpgradeUpperDeck(Transform root, Transform marker, int index, Mats m)
        {
            Transform upper = FindFirst(root, "UpperSleepingDeck");
            if (upper == null) return;

            foreach (Transform pod in upper.GetComponentsInChildren<Transform>(true).Where(t => t.name == "SleepPod").ToArray())
                UpgradeSleepPod(pod, m);

            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            Transform detail = NewGroup(marker, "UpperAccommodationDetails", Vector3.zero, Quaternion.identity);

            // Door frames and open pocket-door leaves keep the room sides visually closed without blocking passage.
            foreach (float rawX in L.CorridorDoorX)
            {
                float x = s * rawX;
                foreach (float side in new[] { -1f, 1f })
                {
                    float z = side * L.CorridorHalf;
                    Cube(detail, "RoomDoorJamb", new Vector3(x - .50f, L.UpperTop + 1.05f, z), new Vector3(.07f, 2.08f, .16f), m.Frame, false);
                    Cube(detail, "RoomDoorJamb", new Vector3(x + .50f, L.UpperTop + 1.05f, z), new Vector3(.07f, 2.08f, .16f), m.Frame, false);
                    Cube(detail, "RoomDoorHeader", new Vector3(x, L.UpperTop + 2.08f, z), new Vector3(1.08f, .08f, .16f), m.Frame, false);
                    Cube(detail, "PocketDoorLeaf", new Vector3(x + s * .72f, L.UpperTop + 1.03f, z + side * .055f), new Vector3(.64f, 1.95f, .055f), m.Wall, false);
                    Cube(detail, "DoorHandle", new Vector3(x + s * .43f, L.UpperTop + 1.02f, z - side * .10f), new Vector3(.08f, .22f, .06f), m.Hazard, false);
                }
            }

            // Corridor ceiling service spine and small night lights.
            Cube(detail, "UpperCeilingSpine", new Vector3(0f, L.RoofBottom - .08f, 0f), new Vector3(L.CarLength - .7f, .12f, .32f), m.Frame, false);
            foreach (float x in new[] { -6f, -2f, 2f, 6f })
            {
                Cube(detail, "NightLight", new Vector3(x, L.UpperTop + .26f, -L.CorridorHalf + .10f), new Vector3(.28f, .10f, .06f), m.WarmLight, false);
                Cube(detail, "NightLight", new Vector3(x, L.UpperTop + .26f, L.CorridorHalf - .10f), new Vector3(.28f, .10f, .06f), m.WarmLight, false);
            }
        }

        private static void UpgradeAccess(Transform root, Transform marker, int index, Mats m)
        {
            Transform access = FindFirst(root, "VerticalAccess");
            if (access == null) return;

            foreach (Renderer r in access.GetComponentsInChildren<Renderer>(true))
            {
                if (r.name.Contains("Rung")) r.sharedMaterial = m.BrushedSteel;
                else r.sharedMaterial = m.Frame;
            }

            Transform d = NewGroup(marker, "AccessSafetyDetails", Vector3.zero, Quaternion.identity);
            float s = L.Cars[index].UpperLaddersAtFront ? -1f : 1f;
            float x0 = s * L.UpperHoleX0;
            float x1 = s * L.UpperHoleX1;
            float innerX = Mathf.Abs(x0) < Mathf.Abs(x1) ? x0 : x1;
            float outerX = Mathf.Abs(x0) > Mathf.Abs(x1) ? x0 : x1;
            float cx = (x0 + x1) * .5f;

            foreach (float side in new[] { -1f, 1f })
            {
                float z0 = side > 0f ? L.UpperHoleZ0 : -L.UpperHoleZ1;
                float z1 = side > 0f ? L.UpperHoleZ1 : -L.UpperHoleZ0;
                float cz = (z0 + z1) * .5f;

                // Bright hatch perimeter makes the opening readable; raised grab handles continue above deck level.
                Cube(d, "UpperHatchInner", new Vector3(innerX, L.UpperTop + .025f, cz), new Vector3(.08f, .04f, Mathf.Abs(z1 - z0)), m.Hazard, false);
                Cube(d, "UpperHatchOuter", new Vector3(outerX, L.UpperTop + .025f, cz), new Vector3(.08f, .04f, Mathf.Abs(z1 - z0)), m.Hazard, false);
                Cube(d, "UpperHatchSideA", new Vector3(cx, L.UpperTop + .025f, z0), new Vector3(Mathf.Abs(x1 - x0), .04f, .08f), m.Hazard, false);
                Cube(d, "UpperHatchSideB", new Vector3(cx, L.UpperTop + .025f, z1), new Vector3(Mathf.Abs(x1 - x0), .04f, .08f), m.Hazard, false);

                float handleX = outerX - s * .20f;
                Cylinder(d, "UpperGrabRailA", new Vector3(handleX, L.UpperTop + .62f, z0 + side * .12f), new Vector3(.055f, 1.18f, .055f), Vector3.zero, m.BrushedSteel, false);
                Cylinder(d, "UpperGrabRailB", new Vector3(handleX, L.UpperTop + .62f, z1 - side * .12f), new Vector3(.055f, 1.18f, .055f), Vector3.zero, m.BrushedSteel, false);
                Cylinder(d, "UpperGrabTop", new Vector3(handleX, L.UpperTop + 1.18f, cz), new Vector3(.055f, Mathf.Abs(z1 - z0) - .24f, .055f), new Vector3(90f, 0f, 0f), m.BrushedSteel, false);
            }

            if (L.Cars[index].TechnicalLadder)
            {
                float tx0 = L.TechnicalHoleX0, tx1 = L.TechnicalHoleX1;
                float tz0 = L.TechnicalHoleZ0, tz1 = L.TechnicalHoleZ1;
                float tcx = (tx0 + tx1) * .5f, tcz = (tz0 + tz1) * .5f;
                Cube(d, "TechHatchA", new Vector3(tx0, L.MainTop + .025f, tcz), new Vector3(.08f, .04f, tz1 - tz0), m.Hazard, false);
                Cube(d, "TechHatchB", new Vector3(tx1, L.MainTop + .025f, tcz), new Vector3(.08f, .04f, tz1 - tz0), m.Hazard, false);
                Cube(d, "TechHatchC", new Vector3(tcx, L.MainTop + .025f, tz0), new Vector3(tx1 - tx0, .04f, .08f), m.Hazard, false);
                Cube(d, "TechHatchD", new Vector3(tcx, L.MainTop + .025f, tz1), new Vector3(tx1 - tx0, .04f, .08f), m.Hazard, false);
                Cylinder(d, "TechGrabRail", new Vector3(tx0 + .15f, L.MainTop + .60f, tz1 + .12f), new Vector3(.055f, 1.10f, .055f), Vector3.zero, m.BrushedSteel, false);
            }
        }

        private static void UpgradeExterior(Transform root, Transform marker, int index, Mats m)
        {
            Transform ext = NewGroup(marker, "ExteriorProductionDetails", Vector3.zero, Quaternion.identity);

            // Long belt rails and vertical panel seams visually join the three-storey body into one train shell.
            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * (L.HalfW + .115f);
                Cube(ext, "ExteriorBelt", new Vector3(0f, 2.73f, z), new Vector3(L.CarLength - .55f, .12f, .07f), m.BrushedSteel, false);
                Cube(ext, "ExteriorLowerBelt", new Vector3(0f, -2.82f, z), new Vector3(L.CarLength - .55f, .10f, .07f), m.Frame, false);
                foreach (float x in new[] { -6f, -3f, 0f, 3f, 6f })
                    Cube(ext, "ExteriorPanelSeam", new Vector3(x, 1.55f, z), new Vector3(.045f, 8.80f, .045f), m.Frame, false);

                // Identification plaque; number bars differ per car even without font assets.
                float plateX = -6.7f;
                Cube(ext, "CarIDPlate", new Vector3(plateX, .72f, z + side * .02f), new Vector3(1.40f, .48f, .05f), m.Frame, false);
                for (int bar = 0; bar <= index; bar++)
                    Cube(ext, "CarIDBar", new Vector3(plateX - .48f + bar * .22f, .72f, z + side * .055f), new Vector3(.10f, .30f, .025f), m.Hazard, false);
            }

            // Visible wheels and suspension detail around the builder bogie blocks.
            foreach (float bx in new[] { -L.HalfL + 3f, L.HalfL - 3f })
                foreach (float wheelX in new[] { bx - .72f, bx + .72f })
                    foreach (float side in new[] { -1f, 1f })
                    {
                        float z = side * 2.85f;
                        Cylinder(ext, "Wheel", new Vector3(wheelX, L.RailTopY + .24f, z), new Vector3(.82f, .28f, .82f), new Vector3(90f, 0f, 0f), m.Frame, false);
                        Cylinder(ext, "WheelRim", new Vector3(wheelX, L.RailTopY + .24f, z + side * .15f), new Vector3(.56f, .045f, .56f), new Vector3(90f, 0f, 0f), m.BrushedSteel, false);
                    }

            foreach (float x in new[] { -4.5f, 0f, 4.5f })
            {
                Cube(ext, "RoofVentBase", new Vector3(x, L.RoofY + .23f, 0f), new Vector3(1.25f, .22f, 1.55f), m.Exterior, false);
                Cube(ext, "RoofVentCap", new Vector3(x, L.RoofY + .39f, 0f), new Vector3(1.52f, .10f, 1.80f), m.Frame, false);
            }

            // Coupler mass below the centre opening at both ends, not in the passenger passage.
            foreach (float sign in new[] { -1f, 1f })
            {
                Cylinder(ext, "CouplerShaft", new Vector3(sign * (L.HalfL + .34f), -2.48f, 0f), new Vector3(.18f, .68f, .18f), new Vector3(0f, 0f, 90f), m.Frame, false);
                Cube(ext, "CouplerHead", new Vector3(sign * (L.HalfL + .72f), -2.48f, 0f), new Vector3(.38f, .34f, .58f), m.Frame, false);
            }

            if (index == 0)
            {
                // Cab nose cheek panels soften the flat prototype front while keeping the panoramic glass clear.
                foreach (float side in new[] { -1f, 1f })
                {
                    GameObject cheek = Cube(ext, "CabNoseCheek", new Vector3(-L.HalfL - .40f, .42f, side * 2.55f), new Vector3(1.05f, .92f, 3.05f), m.Exterior, false);
                    cheek.transform.localRotation = Quaternion.Euler(0f, side * 7f, 0f);
                }
                Cube(ext, "CabNoseLower", new Vector3(-L.HalfL - .46f, .25f, 0f), new Vector3(1.18f, .62f, 2.25f), m.Exterior, false);
            }
        }

        // ---- Locomotive ---------------------------------------------------------------------

        private static void UpgradeLocomotive(Transform root, Transform marker, Mats m)
        {
            Transform console = FindFirst(root, "MainConsole");
            if (console != null)
            {
                SetMaterial(console, m.Frame);
                Transform d = ResetSiblingDetailRoot(console, marker, "LocomotiveConsole");
                Cube(d, "ConsoleTop", new Vector3(.07f, .58f, 0f), new Vector3(1.06f, .16f, 5.15f), m.BrushedSteel, false);
                foreach (float z in new[] { -1.82f, -.91f, 0f, .91f, 1.82f })
                {
                    Cube(d, "ScreenBezel", new Vector3(-.53f, .42f, z), new Vector3(.08f, .58f, .74f), m.Frame, false);
                    Cube(d, "ScreenGlow", new Vector3(-.575f, .43f, z), new Vector3(.025f, .46f, .60f), m.Screen, false);
                    for (int b = -1; b <= 1; b++)
                        Cylinder(d, "ConsoleButton", new Vector3(-.58f, .06f, z + b * .17f), new Vector3(.08f, .035f, .08f), new Vector3(0f, 0f, 90f), b == 0 ? m.Hazard : m.WarmLight, false);
                }
                Cylinder(d, "ThrottleBase", new Vector3(-.15f, .21f, 2.18f), new Vector3(.22f, .08f, .22f), Vector3.zero, m.BrushedSteel, false);
                Cylinder(d, "ThrottleLever", new Vector3(-.26f, .48f, 2.18f), new Vector3(.06f, .62f, .06f), new Vector3(0f, 0f, -18f), m.Frame, false);
                Sphere(d, "ThrottleGrip", new Vector3(-.36f, .78f, 2.18f), new Vector3(.18f, .18f, .18f), m.Rubber, false);
            }

            foreach (Transform seat in FindAll(root, "DriverSeat"))
                RebuildDriverSeat(seat, m);

            foreach (Transform display in FindAll(root, "MainDisplay")) SetMaterial(display, m.Screen);
            foreach (Transform cabinet in FindAll(root, "EquipmentCabinet")) UpgradeCabinet(cabinet, m, m.Hazard);

            Transform terminal = FindFirst(root, "ManagementTerminal");
            if (terminal != null)
            {
                Transform screen = FindFirst(terminal, "TerminalScreen");
                if (screen != null) SetMaterial(screen, m.Screen);
            }

            // Technical traction components get heavy cabling and cooling details.
            foreach (string name in new[] { "TractionMotorA", "TractionMotorB", "TractionBatteryBank", "DriveInverter" })
            {
                Transform machine = FindFirst(root, name);
                if (machine == null) continue;
                Transform d = ResetDetailRoot(machine, "__ProductionMachine");
                for (int i = -2; i <= 2; i++)
                    Cube(d, "CoolingFin", new Vector3(i * .34f, .12f, -.72f), new Vector3(.08f, .82f, .10f), m.BrushedSteel, false);
                Cylinder(d, "PowerCable", new Vector3(.55f, -.42f, .67f), new Vector3(.10f, .70f, .10f), new Vector3(0f, 0f, 90f), m.Copper, false);
            }
        }

        private static void RebuildDriverSeat(Transform seat, Mats m)
        {
            ClearChildren(seat);
            Cube(seat, "SeatPedestal", new Vector3(.03f, .28f, 0f), new Vector3(.38f, .52f, .52f), m.Frame, true);
            Cube(seat, "SeatCushion", new Vector3(-.03f, .62f, 0f), new Vector3(.72f, .18f, .72f), m.Fabric, true);
            Cube(seat, "SeatBackShell", new Vector3(.31f, 1.20f, 0f), new Vector3(.14f, 1.04f, .76f), m.Frame, true);
            Cube(seat, "SeatBackPad", new Vector3(.23f, 1.20f, 0f), new Vector3(.10f, .92f, .66f), m.Fabric, false);
            Cube(seat, "Headrest", new Vector3(.28f, 1.78f, 0f), new Vector3(.15f, .26f, .54f), m.Fabric, false);
            foreach (float z in new[] { -.43f, .43f })
            {
                Cylinder(seat, "ArmPost", new Vector3(-.02f, .92f, z), new Vector3(.05f, .46f, .05f), Vector3.zero, m.BrushedSteel, false);
                Cube(seat, "ArmPad", new Vector3(-.10f, 1.14f, z), new Vector3(.40f, .08f, .11f), m.Rubber, false);
            }
        }

        // ---- Generator ----------------------------------------------------------------------

        private static void UpgradeGenerator(Transform root, Transform marker, Mats m)
        {
            Transform engine = FindFirst(root, "MainGeneratorEngine");
            if (engine != null)
            {
                SetMaterialRecursive(engine, m.Generator);
                Transform d = ResetDetailRoot(engine, "__ProductionGenerator");
                for (int i = -3; i <= 3; i++)
                {
                    float x = i * .58f;
                    Cylinder(d, "CylinderHead", new Vector3(x, .36f, -.84f), new Vector3(.32f, .20f, .32f), new Vector3(90f, 0f, 0f), m.BrushedSteel, false);
                    Cylinder(d, "InjectorLine", new Vector3(x, .66f, -.76f), new Vector3(.045f, .58f, .045f), Vector3.zero, m.Copper, false);
                }
                Cylinder(d, "ExhaustManifold", new Vector3(0f, .77f, -.98f), new Vector3(.16f, 4.10f, .16f), new Vector3(0f, 0f, 90f), m.Frame, false);
                Cube(d, "ServicePanel", new Vector3(0f, .02f, -.82f), new Vector3(2.6f, .76f, .06f), m.Frame, false);
                for (int i = -2; i <= 2; i++)
                    Cylinder(d, "ServiceBolt", new Vector3(i * .48f, .02f, -.86f), new Vector3(.08f, .035f, .08f), new Vector3(90f, 0f, 0f), m.Hazard, false);
            }

            Transform alt = FindFirst(root, "Alternator");
            if (alt != null)
            {
                SetMaterialRecursive(alt, m.Frame);
                Transform d = ResetDetailRoot(alt, "__ProductionAlternator");
                Cylinder(d, "RotorHousing", new Vector3(0f, .05f, -.70f), new Vector3(1.18f, 2.70f, 1.18f), new Vector3(0f, 0f, 90f), m.BrushedSteel, false);
                for (int i = -4; i <= 4; i++)
                    Cube(d, "CoolingSlot", new Vector3(i * .26f, .10f, -1.30f), new Vector3(.10f, .64f, .05f), m.Frame, false);
            }

            foreach (string tankName in new[] { "FuelTankA", "FuelTankB" })
            {
                Transform tank = FindFirst(root, tankName);
                if (tank == null) continue;
                SetMaterial(tank, m.Steel);
                Transform d = ResetSiblingDetailRoot(tank, marker, tankName + "Details");
                foreach (float x in new[] { -1.30f, 1.30f })
                    Cube(d, "TankBand", new Vector3(x, 0f, 0f), new Vector3(.12f, 1.72f, 1.38f), m.Frame, false);
                Cylinder(d, "TankGauge", new Vector3(1.55f, .30f, -.70f), new Vector3(.30f, .05f, .30f), new Vector3(90f, 0f, 0f), m.WhiteEnamel, false);
                Cylinder(d, "TankValve", new Vector3(-1.55f, .58f, -.70f), new Vector3(.20f, .06f, .20f), new Vector3(90f, 0f, 0f), m.Hazard, false);
            }

            Transform console = FindFirst(root, "GeneratorConsole");
            if (console != null) SetMaterial(console, m.Screen);
            Transform rack = FindFirst(root, "MonitoringRack");
            if (rack != null) UpgradeMonitoringRack(rack, m);
            Transform manifold = FindFirst(root, "GeneratorManifold");
            if (manifold != null) UpgradeCabinet(manifold, m, m.Hazard);
        }

        private static void UpgradeMonitoringRack(Transform rack, Mats m)
        {
            SetMaterial(rack, m.Frame);
            Transform d = ResetSiblingDetailRoot(rack, null, "MonitoringRackDetails");
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 4; col++)
                {
                    float x = -1.55f + col * 1.02f;
                    float y = -.52f + row * .58f;
                    Cube(d, "RackScreen", new Vector3(x, y, -.39f), new Vector3(.72f, .34f, .035f), m.Screen, false);
                    Cylinder(d, "RackLamp", new Vector3(x + .34f, y - .19f, -.42f), new Vector3(.07f, .025f, .07f), new Vector3(90f, 0f, 0f), row == 2 ? m.Hazard : m.WarmLight, false);
                }
        }

        // ---- Living -------------------------------------------------------------------------

        private static void UpgradeLiving(Transform root, Transform marker, Mats m)
        {
            foreach (Transform bay in FindAll(root, "SeatingBay")) RebuildSeatingBay(bay, m);

            Transform galley = FindFirst(root, "GalleyCounter");
            if (galley != null)
            {
                SetMaterial(galley, m.WhiteEnamel);
                Transform d = ResetSiblingDetailRoot(galley, marker, "GalleyDetails");
                Cube(d, "CabinetDoorA", new Vector3(-.72f, -.06f, -.47f), new Vector3(1.05f, .68f, .035f), m.WhiteEnamel, false);
                Cube(d, "CabinetDoorB", new Vector3(.72f, -.06f, -.47f), new Vector3(1.05f, .68f, .035f), m.WhiteEnamel, false);
                Cylinder(d, "HandleA", new Vector3(-.18f, .03f, -.51f), new Vector3(.035f, .28f, .035f), new Vector3(90f, 0f, 0f), m.Frame, false);
                Cylinder(d, "HandleB", new Vector3(.18f, .03f, -.51f), new Vector3(.035f, .28f, .035f), new Vector3(90f, 0f, 0f), m.Frame, false);
                Cube(d, "Sink", new Vector3(.54f, .48f, 0f), new Vector3(.78f, .07f, .58f), m.Rubber, false);
                Cylinder(d, "FaucetStem", new Vector3(.70f, .70f, .13f), new Vector3(.035f, .37f, .035f), Vector3.zero, m.BrushedSteel, false);
                Cylinder(d, "FaucetSpout", new Vector3(.55f, .88f, .13f), new Vector3(.035f, .30f, .035f), new Vector3(0f, 0f, 90f), m.BrushedSteel, false);
            }

            Transform galleyTop = FindFirst(root, "GalleyTop");
            if (galleyTop != null) SetMaterial(galleyTop, m.Table);

            Transform medical = FindFirst(root, "MedicalCabinet");
            if (medical != null)
            {
                SetMaterialRecursive(medical, m.WhiteEnamel);
                Transform d = ResetDetailRoot(medical, "__ProductionMedical");
                Cube(d, "MedicalCrossV", new Vector3(0f, .18f, -.36f), new Vector3(.13f, .62f, .035f), m.Hazard, false);
                Cube(d, "MedicalCrossH", new Vector3(0f, .18f, -.36f), new Vector3(.50f, .13f, .035f), m.Hazard, false);
                Cylinder(d, "DoorHandle", new Vector3(.44f, -.48f, -.38f), new Vector3(.035f, .28f, .035f), Vector3.zero, m.Frame, false);
            }

            Transform heater = FindFirst(root, "WallHeater");
            if (heater != null)
            {
                SetMaterial(heater, m.Frame);
                Transform d = ResetSiblingDetailRoot(heater, marker, "HeaterDetails");
                for (int i = -5; i <= 5; i++)
                    Cube(d, "HeaterSlat", new Vector3(i * .105f, .03f, -.19f), new Vector3(.045f, .65f, .035f), m.BrushedSteel, false);
                Cylinder(d, "PipeIn", new Vector3(-.48f, -.49f, 0f), new Vector3(.06f, .26f, .06f), Vector3.zero, m.Copper, false);
                Cylinder(d, "PipeOut", new Vector3(.48f, -.49f, 0f), new Vector3(.06f, .26f, .06f), Vector3.zero, m.Copper, false);
            }

            Transform living = FindFirst(root, "Living_Interior");
            if (living != null)
            {
                Transform d = NewGroup(marker, "LivingWallFurniture", Vector3.zero, Quaternion.identity);
                // Tie luggage racks into the side walls instead of leaving their backs
                // half a metre away from the wall with no visible fasteners.
                foreach (float side in new[] { -1f, 1f })
                    foreach (float x in new[] { -5.5f, -1.5f, 2.4f })
                    {
                        float z = side * (L.HalfW - .32f);
                        Cube(d, "LuggageShelf", new Vector3(x, 2.55f, z),
                            new Vector3(2.55f, .08f, .34f), m.BrushedSteel, false);
                        Cylinder(d, "ShelfRail", new Vector3(x, 2.78f, z - side * .18f),
                            new Vector3(.035f, 2.35f, .035f), new Vector3(0f, 0f, 90f), m.Frame, false);
                        foreach (float edge in new[] { -1.08f, 1.08f })
                        {
                            Cube(d, "ShelfWallBracket", new Vector3(x + edge, 2.42f, side * (L.HalfW - .22f)),
                                new Vector3(.06f, .24f, .32f), m.Frame, false);
                            Cube(d, "ShelfRailPost", new Vector3(x + edge, 2.68f, z - side * .18f),
                                new Vector3(.045f, .25f, .045f), m.Frame, false);
                        }
                    }
            }
        }

        private static void RebuildSeatingBay(Transform bay, Mats m)
        {
            ClearChildren(bay);
            CreateDoubleSeat(bay, new Vector3(-1.10f, 0f, 0f), 0f, m);
            CreateDoubleSeat(bay, new Vector3(1.10f, 0f, 0f), 180f, m);
            Transform table = NewGroup(bay, "DiningTable_Production", Vector3.zero, Quaternion.identity);
            Cube(table, "TableTop", new Vector3(0f, .735f, 0f), new Vector3(1.06f, .075f, 1.42f), m.Table, true);
            Cube(table, "TableEdgeL", new Vector3(0f, .69f, -.70f), new Vector3(1.08f, .11f, .055f), m.BrushedSteel, false);
            Cube(table, "TableEdgeR", new Vector3(0f, .69f, .70f), new Vector3(1.08f, .11f, .055f), m.BrushedSteel, false);
            Cylinder(table, "Pedestal", new Vector3(0f, .37f, 0f), new Vector3(.15f, .64f, .15f), Vector3.zero, m.BrushedSteel, true);
            Cylinder(table, "FloorPlate", new Vector3(0f, .07f, 0f), new Vector3(.42f, .07f, .42f), Vector3.zero, m.BrushedSteel, true);
        }

        private static void CreateDoubleSeat(Transform parent, Vector3 position, float yaw, Mats m)
        {
            Transform seat = NewGroup(parent, "DoubleTrainSeat_Production", position, Quaternion.Euler(0f, yaw, 0f));
            Cube(seat, "UnderShell", new Vector3(0f, .31f, 0f), new Vector3(.70f, .34f, 1.72f), m.Frame, true);
            Cube(seat, "CrossBeam", new Vector3(-.06f, .18f, 0f), new Vector3(.18f, .18f, 1.82f), m.BrushedSteel, true);
            foreach (float z in new[] { -.43f, .43f })
            {
                Cube(seat, "SeatCushion", new Vector3(-.03f, .57f, z), new Vector3(.72f, .16f, .76f), m.Fabric, true);
                Cube(seat, "BackCushion", new Vector3(-.34f, 1.08f, z), new Vector3(.15f, .90f, .75f), m.Fabric, true);
                Cube(seat, "Headrest", new Vector3(-.37f, 1.53f, z), new Vector3(.13f, .25f, .57f), m.Fabric, false);
                Cylinder(seat, "Pedestal", new Vector3(.02f, .21f, z), new Vector3(.10f, .34f, .10f), Vector3.zero, m.BrushedSteel, true);
            }
            foreach (float z in new[] { -.88f, 0f, .88f })
            {
                Cylinder(seat, "ArmrestPost", new Vector3(-.02f, .82f, z), new Vector3(.055f, .38f, .055f), Vector3.zero, m.BrushedSteel, false);
                Cube(seat, "ArmrestPad", new Vector3(.08f, 1.02f, z), new Vector3(.34f, .08f, .10f), m.Rubber, false);
            }
            Cube(seat, "RearShell", new Vector3(-.43f, .98f, 0f), new Vector3(.08f, 1.24f, 1.74f), m.Frame, true);
        }

        // ---- Workshop -----------------------------------------------------------------------

        private static void UpgradeWorkshop(Transform root, Transform marker, Mats m)
        {
            foreach (Transform bench in FindAll(root, "Workbench"))
                RebuildWorkbench(bench, m);

            Transform tool = FindFirst(root, "ToolCabinet");
            if (tool != null) UpgradeCabinet(tool, m, m.Workshop);

            Transform machine = FindFirst(root, "MachineStation");
            if (machine != null)
            {
                SetMaterial(machine, m.Frame);
                Transform d = ResetSiblingDetailRoot(machine, marker, "MachineStationDetails");
                Cube(d, "MachineFace", new Vector3(0f, .18f, -.70f), new Vector3(1.55f, .92f, .05f), m.BrushedSteel, false);
                Cube(d, "MachineScreen", new Vector3(0f, .36f, -.735f), new Vector3(.70f, .36f, .03f), m.Screen, false);
                for (int i = -2; i <= 2; i++)
                    Cylinder(d, "MachineButton", new Vector3(i * .20f, -.18f, -.75f), new Vector3(.09f, .025f, .09f), new Vector3(90f, 0f, 0f), i == 0 ? m.Hazard : m.WarmLight, false);
                Cube(d, "MachineTray", new Vector3(0f, -.58f, -.85f), new Vector3(1.10f, .08f, .36f), m.BrushedSteel, false);
            }

            foreach (string name in new[] { "Compressor", "HydraulicUnit" })
            {
                Transform unit = FindFirst(root, name);
                if (unit == null) continue;
                SetMaterialRecursive(unit, m.Frame);
                Transform d = ResetDetailRoot(unit, "__ProductionUnit");
                Cylinder(d, "PressureTank", new Vector3(-.65f, .05f, -.68f), new Vector3(.72f, 1.12f, .72f), new Vector3(0f, 0f, 90f), m.BrushedSteel, false);
                Cylinder(d, "Gauge", new Vector3(.48f, .38f, -.70f), new Vector3(.28f, .045f, .28f), new Vector3(90f, 0f, 0f), m.WhiteEnamel, false);
                Cylinder(d, "Hose", new Vector3(.72f, -.22f, -.70f), new Vector3(.08f, .62f, .08f), new Vector3(0f, 0f, 90f), m.Rubber, false);
            }
        }

        private static void RebuildWorkbench(Transform bench, Mats m)
        {
            ClearChildren(bench);
            const float length = 5.05f;
            Cube(bench, "WorkbenchBase", new Vector3(0f, .44f, 0f), new Vector3(length, .82f, .90f), m.Frame, true);
            Cube(bench, "WorkbenchTop", new Vector3(0f, .90f, 0f), new Vector3(length + .08f, .10f, .98f), m.BrushedSteel, true);
            Cube(bench, "PegBoard", new Vector3(0f, 1.50f, -.43f), new Vector3(length, 1.02f, .08f), m.Workshop, false);
            for (int i = -4; i <= 4; i++)
                Cube(bench, "Drawer", new Vector3(i * .50f, .42f, -.47f), new Vector3(.42f, .28f, .035f), m.Steel, false);
            for (int i = -3; i <= 3; i++)
            {
                Cylinder(bench, "ToolHandle", new Vector3(i * .58f, 1.48f, -.50f), new Vector3(.055f, .42f, .055f), new Vector3(0f, 0f, i % 2 == 0 ? 18f : -18f), i % 2 == 0 ? m.Hazard : m.Rubber, false);
                Cube(bench, "ToolHead", new Vector3(i * .58f, 1.70f, -.50f), new Vector3(.22f, .08f, .06f), m.BrushedSteel, false);
            }
            // Vice at one end.
            Cube(bench, "ViceBase", new Vector3(-1.75f, 1.03f, -.12f), new Vector3(.42f, .18f, .42f), m.Workshop, false);
            Cube(bench, "ViceJaw", new Vector3(-1.75f, 1.20f, -.25f), new Vector3(.48f, .20f, .12f), m.BrushedSteel, false);
            Cylinder(bench, "ViceHandle", new Vector3(-1.75f, 1.07f, -.50f), new Vector3(.04f, .72f, .04f), new Vector3(90f, 0f, 0f), m.BrushedSteel, false);
        }

        // ---- Greenhouse ---------------------------------------------------------------------

        private static void UpgradeGreenhouse(Transform root, Transform marker, Mats m)
        {
            foreach (Transform bed in FindAll(root, "HydroponicBed")) RebuildHydroponicBed(bed, m);

            foreach (string tankName in new[] { "WaterTank", "NutrientTank" })
            {
                Transform tank = FindFirst(root, tankName);
                if (tank == null) continue;
                SetMaterial(tank, m.BrushedSteel);
                Transform d = ResetSiblingDetailRoot(tank, marker, tankName + "Details");
                foreach (float x in new[] { -1.35f, 1.35f })
                    Cube(d, "TankBand", new Vector3(x, 0f, 0f), new Vector3(.12f, 1.78f, 1.42f), m.Green, false);
                Cylinder(d, "LevelGauge", new Vector3(1.70f, .22f, -.72f), new Vector3(.18f, .05f, .18f), new Vector3(90f, 0f, 0f), m.WhiteEnamel, false);
                Cube(d, "TankLabel", new Vector3(0f, .48f, -.73f), new Vector3(.92f, .30f, .03f), tankName == "WaterTank" ? m.Screen : m.Hazard, false);
            }

            Transform pump = FindFirst(root, "PumpUnit");
            if (pump != null)
            {
                SetMaterialRecursive(pump, m.Green);
                Transform d = ResetDetailRoot(pump, "__ProductionPump");
                Cylinder(d, "PumpMotor", new Vector3(-.55f, .12f, -.68f), new Vector3(.78f, 1.15f, .78f), new Vector3(0f, 0f, 90f), m.Frame, false);
                Cylinder(d, "PumpPipeIn", new Vector3(.50f, -.24f, -.72f), new Vector3(.10f, .90f, .10f), new Vector3(0f, 0f, 90f), m.BrushedSteel, false);
                Cylinder(d, "PumpValve", new Vector3(.88f, .18f, -.72f), new Vector3(.28f, .06f, .28f), new Vector3(90f, 0f, 0f), m.Hazard, false);
            }

            foreach (string name in new[] { "IrrigationManifold", "ClimatePlant" })
            {
                Transform cabinet = FindFirst(root, name);
                if (cabinet != null) UpgradeCabinet(cabinet, m, m.Green);
            }

            Transform panel = FindFirst(root, "ClimatePanel");
            if (panel != null) SetMaterial(panel, m.Frame);
            Transform screen = FindFirst(root, "ClimateScreen");
            if (screen != null) SetMaterial(screen, m.Screen);

            Transform g = NewGroup(marker, "GreenhouseOverhead", Vector3.zero, Quaternion.identity);
            foreach (float x in new[] { -5f, -2.5f, 0f, 2.5f, 5f })
            {
                Cylinder(g, "IrrigationHeader", new Vector3(x, 2.62f, 0f), new Vector3(.06f, 5.5f, .06f), new Vector3(90f, 0f, 0f), m.BrushedSteel, false);
                foreach (float z in new[] { -2.55f, 2.55f })
                    Cylinder(g, "MistDrop", new Vector3(x, 2.35f, z), new Vector3(.035f, .50f, .035f), Vector3.zero, m.BrushedSteel, false);
            }
        }

        private static void RebuildHydroponicBed(Transform bed, Mats m)
        {
            ClearChildren(bed);
            Cube(bed, "BedFrame", new Vector3(0f, .40f, 0f), new Vector3(1.90f, .80f, 1.28f), m.Frame, true);
            Cube(bed, "Reservoir", new Vector3(0f, .72f, 0f), new Vector3(1.72f, .30f, 1.08f), m.BrushedSteel, false);
            Cube(bed, "PlantTray", new Vector3(0f, .98f, 0f), new Vector3(1.68f, .12f, 1.08f), m.Green, false);
            foreach (float x in new[] { -.60f, -.30f, 0f, .30f, .60f })
            {
                Cylinder(bed, "PlantStem", new Vector3(x, 1.23f, 0f), new Vector3(.035f, .42f, .035f), Vector3.zero, m.Plant, false);
                Sphere(bed, "LeafA", new Vector3(x - .10f, 1.43f, -.06f), new Vector3(.24f, .12f, .34f), m.Plant, false);
                Sphere(bed, "LeafB", new Vector3(x + .10f, 1.36f, .08f), new Vector3(.28f, .12f, .30f), m.Plant, false);
            }
            foreach (float z in new[] { -.50f, .50f })
            {
                Cylinder(bed, "GrowPost", new Vector3(-.72f, 1.75f, z), new Vector3(.045f, 1.55f, .045f), Vector3.zero, m.Frame, false);
                Cylinder(bed, "GrowPost", new Vector3(.72f, 1.75f, z), new Vector3(.045f, 1.55f, .045f), Vector3.zero, m.Frame, false);
            }
            Cube(bed, "GrowLightHousing", new Vector3(0f, 2.48f, 0f), new Vector3(1.74f, .10f, .30f), m.Frame, false);
            Cube(bed, "GrowLight", new Vector3(0f, 2.42f, 0f), new Vector3(1.55f, .03f, .16f), m.WarmLight, false);
            Cylinder(bed, "FeedPipe", new Vector3(0f, .60f, -.62f), new Vector3(.055f, 1.55f, .055f), new Vector3(0f, 0f, 90f), m.BrushedSteel, false);
        }

        // ---- Common furniture helpers -------------------------------------------------------

        private static void UpgradeSleepPod(Transform pod, Mats m)
        {
            Transform mattress = FindFirst(pod, "Mattress");
            if (mattress != null) SetMaterial(mattress, m.Bedding);
            Transform bedFrame = FindFirst(pod, "BedFrame");
            if (bedFrame != null) SetMaterial(bedFrame, m.Frame);
            Transform locker = FindFirst(pod, "Locker");
            if (locker != null) SetMaterial(locker, m.Steel);
            Transform shelf = FindFirst(pod, "Shelf");
            if (shelf != null) SetMaterial(shelf, m.BrushedSteel);

            Transform d = ResetDetailRoot(pod, "__ProductionSleepPod");
            if (bedFrame == null) return;

            // SleepPod is a zero-origin group. Its BedFrame is offset to the actual
            // cabin (x, upper deck Y, +/-z). Previously these details stayed at
            // car-local (0, 0, 0), appearing in empty space far below the beds.
            Vector3 frameCentre = bedFrame.localPosition;
            d.localPosition = frameCentre - Vector3.up * .22f;
            float side = Mathf.Sign(frameCentre.z);
            float aisleFacingZ = -side * .42f;

            Cube(d, "Pillow", new Vector3(-.72f, .67f, 0f),
                new Vector3(.48f, .16f, .64f), m.Bedding, false);
            Cube(d, "FoldedBlanket", new Vector3(.48f, .65f, 0f),
                new Vector3(.68f, .10f, .72f), m.Fabric, false);

            // Guard rail, short uprights and reading lamp are all attached to
            // the actual bed frame rather than left suspended in the corridor.
            Cylinder(d, "SafetyRail", new Vector3(.18f, .83f, aisleFacingZ),
                new Vector3(.035f, 1.10f, .035f), new Vector3(0f, 0f, 90f),
                m.BrushedSteel, false);
            foreach (float x in new[] { -.32f, .68f })
                Cylinder(d, "SafetyRailPost", new Vector3(x, .64f, aisleFacingZ),
                    new Vector3(.035f, .40f, .035f), Vector3.zero, m.Frame, false);

            Cube(d, "ReadingLightPost", new Vector3(-.92f, .96f, aisleFacingZ),
                new Vector3(.045f, 1.02f, .045f), m.Frame, false);
            Cube(d, "ReadingLight", new Vector3(-.92f, 1.47f, aisleFacingZ),
                new Vector3(.26f, .10f, .18f), m.WarmLight, false);
            Cube(d, "PersonalPanel", new Vector3(-.76f, 1.23f, aisleFacingZ),
                new Vector3(.34f, .28f, .03f), m.Frame, false);
            Cylinder(d, "PanelButton", new Vector3(-.76f, 1.23f, aisleFacingZ - side * .025f),
                new Vector3(.07f, .02f, .07f), new Vector3(90f, 0f, 0f), m.Hazard, false);

            if (locker != null)
            {
                // Locker lives in pod-local coordinates; convert to the re-based detail group.
                Vector3 lockerOffset = locker.localPosition - d.localPosition;
                Cylinder(d, "LockerHandle", lockerOffset + new Vector3(0f, .05f, -side * .30f),
                    new Vector3(.03f, .24f, .03f), Vector3.zero, m.Frame, false);
            }
        }

        private static void UpgradeCabinet(Transform cabinet, Mats m, Material accent)
        {
            SetMaterialRecursive(cabinet, m.Frame);
            Transform d = ResetDetailRoot(cabinet, "__ProductionCabinet");
            Cube(d, "CabinetFace", new Vector3(0f, 0f, -.54f), new Vector3(1.55f, 1.34f, .05f), m.Steel, false);
            foreach (float x in new[] { -.36f, .36f })
                Cylinder(d, "CabinetHandle", new Vector3(x, .05f, -.59f), new Vector3(.035f, .34f, .035f), Vector3.zero, m.BrushedSteel, false);
            for (int i = -3; i <= 3; i++)
                Cube(d, "VentSlot", new Vector3(i * .18f, .43f, -.60f), new Vector3(.10f, .035f, .02f), accent, false);
        }

        // ---- Material / texture utilities ---------------------------------------------------

        private static Material Textured(string name, string albedoPath, string normalPath, float metallic, float smoothness, Color tint, float tiling)
        {
            Material mat = GetOrCreateMaterial(name);
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
            if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normal);
            if (mat.HasProperty("_BaseMap")) mat.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
            if (mat.HasProperty("_BumpMap")) mat.SetTextureScale("_BumpMap", new Vector2(tiling, tiling));
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            mat.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material Solid(string name, Color color, float metallic, float smoothness)
        {
            Material mat = GetOrCreateMaterial(name);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material Emissive(string name, Color baseColor, Color emission)
        {
            Material mat = Solid(name, baseColor, .05f, .36f);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
            mat.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material GetOrCreateMaterial(string name)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void EnsureTexture(string path, bool normalMap, Func<int, int, Color> generator)
        {
            string full = Path.GetFullPath(path);
            if (!File.Exists(full))
            {
                Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, normalMap);
                for (int y = 0; y < TextureSize; y++)
                    for (int x = 0; x < TextureSize; x++)
                        texture.SetPixel(x, y, generator(x, y));
                texture.Apply(false, false);
                File.WriteAllBytes(full, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !normalMap;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = TextureSize;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
        }

        private static Color FloorAlbedo(int x, int y)
        {
            float baseV = .105f + (Hash01(x, y) - .5f) * .025f;
            bool ribA = ((x + y) % 28) < 3;
            bool ribB = ((x - y + 1024) % 28) < 3;
            float rib = (ribA || ribB) ? .055f : 0f;
            return new Color(baseV + rib, baseV + rib * .9f, baseV + rib * .75f, 1f);
        }

        private static Color FloorNormal(int x, int y)
        {
            float a = ((x + y) % 28) < 4 ? .16f : 0f;
            float b = ((x - y + 1024) % 28) < 4 ? -.16f : 0f;
            return EncodeNormal(new Vector3(a, b, 1f));
        }

        private static Color WallAlbedo(int x, int y)
        {
            float n = (Hash01(x * 3 + 11, y * 5 + 7) - .5f) * .028f;
            float seam = (x % 128 < 3 || y % 192 < 3) ? -.050f : 0f;
            float scuff = y < 92 ? (Hash01(x + 91, y + 33) > .93f ? -.055f : 0f) : 0f;
            float v = .19f + n + seam + scuff;
            return new Color(v * .88f, v * .96f, v, 1f);
        }

        private static Color WallNormal(int x, int y)
        {
            float sx = x % 128 < 4 ? -.12f : 0f;
            float sy = y % 192 < 4 ? -.08f : 0f;
            return EncodeNormal(new Vector3(sx, sy, 1f));
        }

        private static Color SteelAlbedo(int x, int y)
        {
            float brush = Mathf.Sin(y * .39f) * .014f + (Hash01(x * 2, y + 8) - .5f) * .020f;
            float seam = x % 96 < 2 ? -.035f : 0f;
            float v = .31f + brush + seam;
            return new Color(v * .92f, v * .98f, v, 1f);
        }

        private static Color SteelNormal(int x, int y)
        {
            float nx = Mathf.Sin(y * .39f) * .045f;
            float ny = (Hash01(x + 7, y + 5) - .5f) * .035f;
            return EncodeNormal(new Vector3(nx, ny, 1f));
        }

        private static Color FabricAlbedo(int x, int y)
        {
            float weave = ((x / 3 + y / 3) & 1) == 0 ? .038f : -.026f;
            float thread = (x % 7 == 0 || y % 7 == 0) ? .020f : 0f;
            float n = (Hash01(x + 22, y + 18) - .5f) * .026f;
            return new Color(.11f + weave + n, .18f + weave + thread + n, .22f + weave + thread + n, 1f);
        }

        private static Color BeddingAlbedo(int x, int y)
        {
            float band = ((x + y) / 24) % 2 == 0 ? .026f : -.014f;
            float n = (Hash01(x + 77, y + 19) - .5f) * .018f;
            return new Color(.34f + band + n, .39f + band + n, .40f + band + n, 1f);
        }

        private static Color FabricNormal(int x, int y)
        {
            float nx = Mathf.Sin(x * Mathf.PI / 3.5f) * .095f;
            float ny = Mathf.Sin(y * Mathf.PI / 3.5f) * .095f;
            return EncodeNormal(new Vector3(nx, ny, 1f));
        }

        private static Color TableAlbedo(int x, int y)
        {
            float grain = Mathf.Sin((x + Mathf.Sin(y * .07f) * 9f) * .11f) * .020f;
            float n = (Hash01(x + 12, y + 34) - .5f) * .014f;
            return new Color(.29f + grain + n, .28f + grain + n, .25f + grain + n, 1f);
        }

        private static Color TableNormal(int x, int y)
        {
            float nx = Mathf.Sin((x + Mathf.Sin(y * .07f) * 9f) * .11f) * .055f;
            return EncodeNormal(new Vector3(nx, 0f, 1f));
        }

        private static Color EncodeNormal(Vector3 n)
        {
            n.Normalize();
            return new Color(n.x * .5f + .5f, n.y * .5f + .5f, n.z * .5f + .5f, 1f);
        }

        private static float Hash01(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                return (h & 0x00FFFFFF) / 16777215f;
            }
        }

        // ---- Scene-graph helpers ------------------------------------------------------------

        private static Transform NewGroup(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            GameObject go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
        }

        private static GameObject Cylinder(Transform parent, string name, Vector3 pos, Vector3 size, Vector3 euler, Material mat, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = new Vector3(size.x, size.y * .5f, size.z);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
        }

        private static GameObject Sphere(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
        }

        private static void AddPoint(Transform parent, string name, Vector3 pos, float intensity, float range, Color color)
        {
            Transform t = NewGroup(parent, name, pos, Quaternion.identity);
            Light l = t.gameObject.AddComponent<Light>();
            l.type = LightType.Point;
            l.intensity = intensity;
            l.range = range;
            l.color = color;
            l.shadows = LightShadows.None;
        }

        private static IEnumerable<Transform> FindAll(Transform root, string name) =>
            root.GetComponentsInChildren<Transform>(true).Where(t => t != null && t.name == name).ToArray();

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

        private static void SetMaterial(Transform target, Material mat)
        {
            if (target == null) return;
            Renderer r = target.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
        }

        private static void SetMaterialRecursive(Transform target, Material mat)
        {
            if (target == null) return;
            foreach (Renderer r in target.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial = mat;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        private static Transform ResetDetailRoot(Transform parent, string name)
        {
            Transform old = parent.Find(name);
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            return NewGroup(parent, name, Vector3.zero, Quaternion.identity);
        }

        private static Transform ResetSiblingDetailRoot(Transform target, Transform marker, string suffix)
        {
            Transform parent = marker != null ? marker : target.parent;
            string name = "__Production_" + suffix;
            Transform old = parent != null ? parent.Find(name) : null;
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            return NewGroup(parent, name, target.localPosition, target.localRotation);
        }

        private static void RemoveMarker(Transform root, string name)
        {
            Transform old = root.Find(name);
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        }

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
