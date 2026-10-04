#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Free, deterministic production-art skin pass. Architecture and gameplay placement stay owned by
    /// FrozenTrainLayout/FrozenTrainBuilder; this class only upgrades generated visual children/materials.
    /// It intentionally requires no paid service and no manual Blender work.
    /// </summary>
    public static class FrozenTrainProductionArt
    {
        private const string ArtRoot = "Assets/Art";
        private const string TextureRoot = ArtRoot + "/Textures/Generated";
        private const string MaterialRoot = ArtRoot + "/Materials/Living";
        private const string LivingPrefabPath = "Assets/Prefabs/Train/03_Living.prefab";
        private const int TextureSize = 256;

        private sealed class LivingMaterials
        {
            public Material Fabric;
            public Material Bedding;
            public Material PaintedMetal;
            public Material Table;
            public Material DarkRubber;
            public Material MedicalAccent;
        }

        public static void Apply()
        {
            EnsureFolders();
            LivingMaterials materials = BuildLivingMaterials();
            UpgradeLivingPrefab(materials);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ArtRoot);
            EnsureFolder(ArtRoot + "/Textures");
            EnsureFolder(TextureRoot);
            EnsureFolder(ArtRoot + "/Materials");
            EnsureFolder(MaterialRoot);
        }

        private static LivingMaterials BuildLivingMaterials()
        {
            string fabricA = TextureRoot + "/FT_LIV_Fabric_Albedo.png";
            string fabricN = TextureRoot + "/FT_LIV_Fabric_Normal.png";
            string beddingA = TextureRoot + "/FT_LIV_Bedding_Albedo.png";
            string beddingN = TextureRoot + "/FT_LIV_Bedding_Normal.png";
            string metalA = TextureRoot + "/FT_LIV_PaintedMetal_Albedo.png";
            string metalN = TextureRoot + "/FT_LIV_PaintedMetal_Normal.png";
            string tableA = TextureRoot + "/FT_LIV_Table_Albedo.png";
            string tableN = TextureRoot + "/FT_LIV_Table_Normal.png";

            EnsureTexture(fabricA, false, GenerateFabricAlbedo);
            EnsureTexture(fabricN, true, GenerateFabricNormal);
            EnsureTexture(beddingA, false, GenerateBeddingAlbedo);
            EnsureTexture(beddingN, true, GenerateFabricNormal);
            EnsureTexture(metalA, false, GenerateMetalAlbedo);
            EnsureTexture(metalN, true, GenerateMetalNormal);
            EnsureTexture(tableA, false, GenerateTableAlbedo);
            EnsureTexture(tableN, true, GenerateTableNormal);

            return new LivingMaterials
            {
                Fabric = CreateMaterial("FT_LIV_Fabric", fabricA, fabricN, .02f, .25f, new Color(.74f, .80f, .86f)),
                Bedding = CreateMaterial("FT_LIV_Bedding", beddingA, beddingN, 0f, .20f, new Color(.92f, .94f, .96f)),
                PaintedMetal = CreateMaterial("FT_LIV_PaintedMetal", metalA, metalN, .42f, .34f, new Color(.60f, .65f, .70f)),
                Table = CreateMaterial("FT_LIV_Table", tableA, tableN, .08f, .40f, new Color(.78f, .76f, .70f)),
                DarkRubber = SolidMaterial("FT_LIV_DarkRubber", new Color(.045f, .055f, .060f), 0f, .18f),
                MedicalAccent = SolidMaterial("FT_LIV_MedicalAccent", new Color(.52f, .055f, .045f), .02f, .26f),
            };
        }

        private static void UpgradeLivingPrefab(LivingMaterials m)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(LivingPrefabPath) == null)
            {
                Debug.LogWarning("[ProductionArt] Living prefab not found; run the canonical builder first.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(LivingPrefabPath);
            try
            {
                Transform[] all = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform bay in all.Where(t => t != null && t.name == "SeatingBay").ToArray())
                    UpgradeSeatingBay(bay, m);

                Transform galley = FindFirst(root.transform, "GalleyCounter");
                if (galley != null) UpgradeGalley(galley, m);

                Transform galleyTop = FindFirst(root.transform, "GalleyTop");
                if (galleyTop != null) SetRendererMaterial(galleyTop, m.Table);

                Transform medical = FindFirst(root.transform, "MedicalCabinet");
                if (medical != null) UpgradeMedicalCabinet(medical, m);

                Transform heater = FindFirst(root.transform, "WallHeater");
                if (heater != null) UpgradeHeater(heater, m);

                Transform upper = FindFirst(root.transform, "UpperSleepingDeck");
                if (upper != null)
                    foreach (Transform pod in upper.GetComponentsInChildren<Transform>(true).Where(t => t.name == "SleepPod").ToArray())
                        UpgradeSleepPod(pod, m);

                Transform oldMarker = root.transform.Find("ProductionArt_LivingV1");
                if (oldMarker != null) UnityEngine.Object.DestroyImmediate(oldMarker.gameObject);
                GameObject marker = new GameObject("ProductionArt_LivingV1");
                marker.transform.SetParent(root.transform, false);
                PrefabUtility.SaveAsPrefabAsset(root, LivingPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void UpgradeSeatingBay(Transform bay, LivingMaterials m)
        {
            ClearChildren(bay);
            CreateDoubleSeat(bay, new Vector3(-1.10f, 0f, 0f), 0f, m);
            CreateDoubleSeat(bay, new Vector3(1.10f, 0f, 0f), 180f, m);

            Transform table = NewGroup(bay, "DiningTable_Production", Vector3.zero, Quaternion.identity);
            Cube(table, "TableTop", new Vector3(0f, .735f, 0f), new Vector3(1.06f, .075f, 1.42f), m.Table, true);
            Cube(table, "TableEdgeL", new Vector3(0f, .69f, -.70f), new Vector3(1.08f, .11f, .055f), m.PaintedMetal, false);
            Cube(table, "TableEdgeR", new Vector3(0f, .69f, .70f), new Vector3(1.08f, .11f, .055f), m.PaintedMetal, false);
            Cylinder(table, "Pedestal", new Vector3(0f, .37f, 0f), new Vector3(.15f, .64f, .15f), Vector3.zero, m.PaintedMetal, true);
            Cylinder(table, "FloorPlate", new Vector3(0f, .07f, 0f), new Vector3(.42f, .07f, .42f), Vector3.zero, m.PaintedMetal, true);
        }

        private static void CreateDoubleSeat(Transform parent, Vector3 position, float yaw, LivingMaterials m)
        {
            Transform seat = NewGroup(parent, "DoubleTrainSeat_Production", position, Quaternion.Euler(0f, yaw, 0f));

            Cube(seat, "UnderShell", new Vector3(0f, .31f, 0f), new Vector3(.70f, .34f, 1.72f), m.PaintedMetal, true);
            Cube(seat, "CrossBeam", new Vector3(-.06f, .18f, 0f), new Vector3(.18f, .18f, 1.82f), m.PaintedMetal, true);
            foreach (float z in new[] { -.43f, .43f })
            {
                Cube(seat, "SeatCushion", new Vector3(-.03f, .57f, z), new Vector3(.72f, .16f, .76f), m.Fabric, true);
                Cube(seat, "BackCushion", new Vector3(-.34f, 1.08f, z), new Vector3(.15f, .90f, .75f), m.Fabric, true);
                Cube(seat, "Headrest", new Vector3(-.37f, 1.53f, z), new Vector3(.13f, .25f, .57f), m.Fabric, false);
                Cylinder(seat, "Pedestal", new Vector3(.02f, .21f, z), new Vector3(.10f, .34f, .10f), Vector3.zero, m.PaintedMetal, true);
            }

            foreach (float z in new[] { -.88f, 0f, .88f })
            {
                Cylinder(seat, "ArmrestPost", new Vector3(-.02f, .82f, z), new Vector3(.055f, .38f, .055f), Vector3.zero, m.PaintedMetal, false);
                Cube(seat, "ArmrestPad", new Vector3(.08f, 1.02f, z), new Vector3(.34f, .08f, .10f), m.DarkRubber, false);
            }

            Cube(seat, "RearShell", new Vector3(-.43f, .98f, 0f), new Vector3(.08f, 1.24f, 1.74f), m.PaintedMetal, true);
        }

        private static void UpgradeGalley(Transform galley, LivingMaterials m)
        {
            SetRendererMaterial(galley, m.PaintedMetal);
            Transform detail = ResetSiblingDetailRoot(galley);
            Cube(detail, "CabinetDoorA", new Vector3(-.72f, -.06f, -.47f), new Vector3(1.05f, .68f, .035f), m.PaintedMetal, false);
            Cube(detail, "CabinetDoorB", new Vector3(.72f, -.06f, -.47f), new Vector3(1.05f, .68f, .035f), m.PaintedMetal, false);
            Cylinder(detail, "HandleA", new Vector3(-.18f, .03f, -.51f), new Vector3(.035f, .28f, .035f), new Vector3(90f, 0f, 0f), m.DarkRubber, false);
            Cylinder(detail, "HandleB", new Vector3(.18f, .03f, -.51f), new Vector3(.035f, .28f, .035f), new Vector3(90f, 0f, 0f), m.DarkRubber, false);
            Cube(detail, "Sink", new Vector3(.54f, .48f, 0f), new Vector3(.78f, .07f, .58f), m.DarkRubber, false);
            Cylinder(detail, "FaucetStem", new Vector3(.70f, .70f, .13f), new Vector3(.035f, .37f, .035f), Vector3.zero, m.PaintedMetal, false);
            Cylinder(detail, "FaucetSpout", new Vector3(.55f, .88f, .13f), new Vector3(.035f, .30f, .035f), new Vector3(0f, 0f, 90f), m.PaintedMetal, false);
        }

        private static void UpgradeMedicalCabinet(Transform cabinet, LivingMaterials m)
        {
            foreach (Renderer r in cabinet.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial = m.PaintedMetal;
            Transform detail = ResetDetailRoot(cabinet);
            Cube(detail, "MedicalCrossV", new Vector3(0f, .18f, -.36f), new Vector3(.13f, .62f, .035f), m.MedicalAccent, false);
            Cube(detail, "MedicalCrossH", new Vector3(0f, .18f, -.36f), new Vector3(.50f, .13f, .035f), m.MedicalAccent, false);
            Cylinder(detail, "DoorHandle", new Vector3(.44f, -.48f, -.38f), new Vector3(.035f, .28f, .035f), Vector3.zero, m.DarkRubber, false);
        }

        private static void UpgradeHeater(Transform heater, LivingMaterials m)
        {
            SetRendererMaterial(heater, m.PaintedMetal);
            Transform detail = ResetSiblingDetailRoot(heater);
            for (int i = -5; i <= 5; i++)
                Cube(detail, "HeaterSlat", new Vector3(i * .105f, .03f, -.19f), new Vector3(.045f, .65f, .035f), m.DarkRubber, false);
            Cylinder(detail, "PipeIn", new Vector3(-.48f, -.49f, 0f), new Vector3(.06f, .26f, .06f), Vector3.zero, m.PaintedMetal, false);
            Cylinder(detail, "PipeOut", new Vector3(.48f, -.49f, 0f), new Vector3(.06f, .26f, .06f), Vector3.zero, m.PaintedMetal, false);
        }

        private static void UpgradeSleepPod(Transform pod, LivingMaterials m)
        {
            Transform mattress = FindFirst(pod, "Mattress");
            if (mattress != null) SetRendererMaterial(mattress, m.Bedding);
            Transform bedFrame = FindFirst(pod, "BedFrame");
            if (bedFrame != null) SetRendererMaterial(bedFrame, m.PaintedMetal);
            Transform locker = FindFirst(pod, "Locker");
            if (locker != null) SetRendererMaterial(locker, m.PaintedMetal);

            Transform detail = ResetDetailRoot(pod);
            Cube(detail, "Pillow", new Vector3(-.72f, .67f, 0f), new Vector3(.48f, .16f, .64f), m.Bedding, false);
            Cube(detail, "FoldedBlanket", new Vector3(.48f, .65f, 0f), new Vector3(.68f, .10f, .72f), m.Fabric, false);
            Cylinder(detail, "SafetyRail", new Vector3(.18f, .83f, -.48f), new Vector3(.035f, 1.10f, .035f), new Vector3(0f, 0f, 90f), m.PaintedMetal, false);
            Cube(detail, "ReadingLight", new Vector3(-.92f, 1.47f, -.39f), new Vector3(.26f, .10f, .18f), m.Table, false);
            if (locker != null)
                Cylinder(detail, "LockerHandle", locker.localPosition + new Vector3(0f, .05f, -.30f), new Vector3(.03f, .24f, .03f), Vector3.zero, m.DarkRubber, false);
        }

        private static Transform ResetSiblingDetailRoot(Transform target)
        {
            string name = "__ProductionArt_" + target.name;
            Transform parent = target.parent;
            Transform old = parent != null ? parent.Find(name) : null;
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            return NewGroup(parent, name, target.localPosition, target.localRotation);
        }

        private static Transform ResetDetailRoot(Transform parent)
        {
            Transform old = parent.Find("__ProductionArt");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            return NewGroup(parent, "__ProductionArt", Vector3.zero, Quaternion.identity);
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        private static Transform FindFirst(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = FindFirst(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static void SetRendererMaterial(Transform target, Material material)
        {
            Renderer r = target.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = material;
        }

        private static Transform NewGroup(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
        }

        private static GameObject Cylinder(Transform parent, string name, Vector3 position, Vector3 scale, Vector3 euler, Material material, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = new Vector3(scale.x, scale.y * .5f, scale.z);
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
            return go;
        }

        private static Material CreateMaterial(string name, string albedoPath, string normalPath, float metallic, float smoothness, Color tint)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP/Lit shader not found.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", albedo);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
            if (material.HasProperty("_BumpMap")) material.SetTexture("_BumpMap", normal);
            if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", .55f);
            if (normal != null) material.EnableKeyword("_NORMALMAP");
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material SolidMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP/Lit shader not found.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureTexture(string path, bool normalMap, Func<int, int, Color> generator)
        {
            if (!File.Exists(Path.GetFullPath(path)))
            {
                Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, normalMap);
                for (int y = 0; y < TextureSize; y++)
                    for (int x = 0; x < TextureSize; x++)
                        texture.SetPixel(x, y, generator(x, y));
                texture.Apply(false, false);
                File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
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
                importer.maxTextureSize = 512;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
        }

        private static Color GenerateFabricAlbedo(int x, int y)
        {
            float weave = ((x / 3 + y / 3) & 1) == 0 ? .035f : -.025f;
            float thread = (x % 6 == 0 || y % 6 == 0) ? .025f : 0f;
            float n = Hash01(x, y) * .025f - .0125f;
            return new Color(.13f + weave + n, .19f + weave + thread + n, .23f + weave + thread + n, 1f);
        }

        private static Color GenerateBeddingAlbedo(int x, int y)
        {
            float band = ((x + y) / 18) % 2 == 0 ? .025f : -.015f;
            float n = Hash01(x + 77, y + 19) * .018f - .009f;
            return new Color(.34f + band + n, .39f + band + n, .40f + band + n, 1f);
        }

        private static Color GenerateMetalAlbedo(int x, int y)
        {
            float brush = Mathf.Sin(y * .34f) * .012f + (Hash01(x * 3, y) - .5f) * .018f;
            float seam = x % 64 < 2 ? -.045f : 0f;
            return new Color(.18f + brush + seam, .205f + brush + seam, .215f + brush + seam, 1f);
        }

        private static Color GenerateTableAlbedo(int x, int y)
        {
            float grain = Mathf.Sin((x + Mathf.Sin(y * .07f) * 9f) * .11f) * .018f;
            float n = (Hash01(x + 12, y + 34) - .5f) * .012f;
            return new Color(.31f + grain + n, .30f + grain + n, .27f + grain + n, 1f);
        }

        private static Color GenerateFabricNormal(int x, int y)
        {
            float nx = Mathf.Sin(x * Mathf.PI / 3f) * .10f;
            float ny = Mathf.Sin(y * Mathf.PI / 3f) * .10f;
            Vector3 n = new Vector3(nx, ny, 1f).normalized;
            return new Color(n.x * .5f + .5f, n.y * .5f + .5f, n.z * .5f + .5f, 1f);
        }

        private static Color GenerateMetalNormal(int x, int y)
        {
            float ny = Mathf.Sin(y * .46f) * .035f;
            Vector3 n = new Vector3(0f, ny, 1f).normalized;
            return new Color(.5f, n.y * .5f + .5f, n.z * .5f + .5f, 1f);
        }

        private static Color GenerateTableNormal(int x, int y)
        {
            float nx = Mathf.Cos(x * .11f + Mathf.Sin(y * .07f)) * .045f;
            Vector3 n = new Vector3(nx, 0f, 1f).normalized;
            return new Color(n.x * .5f + .5f, .5f, n.z * .5f + .5f, 1f);
        }

        private static float Hash01(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                return (h & 0x00FFFFFFu) / 16777215f;
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
#endif
