using UnityEngine;
using UnityEngine.Rendering;

namespace FrozenTrain.Atmosphere
{
    /// <summary>
    /// Lightweight visual art pass for the five-car prototype. It deliberately uses
    /// simple primitives and no extra realtime lights so the prototype remains friendly
    /// to lower-end PCs while looking less like a greybox.
    /// </summary>
    public sealed class InteriorPolishRuntime : MonoBehaviour
    {
        private const string DetailRootName = "PRO_INTERIOR_DETAILS";

        private Material darkPanel;
        private Material steel;
        private Material pipe;
        private Material rubber;
        private Material warmAccent;
        private Material cable;
        private Material livingAccent;
        private Material workshopAccent;
        private Material greenhouseAccent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            GameObject train = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (train == null || train.GetComponent<InteriorPolishRuntime>() != null)
            {
                return;
            }

            train.AddComponent<InteriorPolishRuntime>();
        }

        private void Awake()
        {
            if (transform.Find(DetailRootName) != null)
            {
                return;
            }

            CreateMaterials();

            GameObject detailRoot = new GameObject(DetailRootName);
            detailRoot.transform.SetParent(transform, false);

            float[] centers = { -12.4f, 0f, 12.4f, 24.8f, 37.2f };
            for (int i = 0; i < centers.Length; i++)
            {
                CreateCommonCarDetails(detailRoot.transform, centers[i], i);
            }

            CreateLocomotiveDetails(detailRoot.transform);
            CreateGeneratorDetails(detailRoot.transform);
            CreateLivingDetails(detailRoot.transform);
            CreateWorkshopDetails(detailRoot.transform);
            CreateGreenhouseDetails(detailRoot.transform);
            CreateConnectorDetails(detailRoot.transform);
        }

        private void CreateMaterials()
        {
            darkPanel = CreateMaterial("Runtime_DarkPanel", new Color(0.045f, 0.052f, 0.060f), 0.34f, 0.72f);
            steel = CreateMaterial("Runtime_Steel", new Color(0.19f, 0.22f, 0.24f), 0.42f, 0.76f);
            pipe = CreateMaterial("Runtime_Pipe", new Color(0.24f, 0.27f, 0.28f), 0.32f, 0.64f);
            rubber = CreateMaterial("Runtime_Rubber", new Color(0.025f, 0.028f, 0.032f), 0.12f, 0.05f);
            warmAccent = CreateMaterial("Runtime_WarmAccent", new Color(0.48f, 0.19f, 0.035f), 0.28f, 0.22f);
            cable = CreateMaterial("Runtime_Cable", new Color(0.055f, 0.07f, 0.085f), 0.20f, 0.12f);
            livingAccent = CreateMaterial("Runtime_LivingAccent", new Color(0.12f, 0.25f, 0.28f), 0.23f, 0.08f);
            workshopAccent = CreateMaterial("Runtime_WorkshopAccent", new Color(0.31f, 0.22f, 0.10f), 0.24f, 0.30f);
            greenhouseAccent = CreateMaterial("Runtime_GreenhouseAccent", new Color(0.08f, 0.29f, 0.15f), 0.20f, 0.05f);
        }

        private void CreateCommonCarDetails(Transform parent, float centerX, int carIndex)
        {
            GameObject group = new GameObject($"Car_{carIndex + 1:00}_DetailPass");
            group.transform.SetParent(parent, false);

            foreach (float side in new[] { -1f, 1f })
            {
                for (int i = -2; i <= 2; i++)
                {
                    float x = centerX + i * 2.15f;
                    CreateBox(group.transform, "LowerWallPanel", new Vector3(x, 0.83f, side * 2.37f), new Vector3(1.75f, 0.58f, 0.055f), darkPanel);
                    CreateBox(group.transform, "PanelTopRail", new Vector3(x, 1.13f, side * 2.34f), new Vector3(1.82f, 0.055f, 0.07f), steel);
                }

                CreateBox(group.transform, "CableChannel", new Vector3(centerX, 2.84f, side * 2.22f), new Vector3(10.9f, 0.16f, 0.18f), cable);
                for (int i = -2; i <= 2; i++)
                {
                    CreateBox(group.transform, "CableClamp", new Vector3(centerX + i * 2.25f, 2.84f, side * 2.11f), new Vector3(0.09f, 0.24f, 0.12f), steel);
                }
            }

            CreateBox(group.transform, "CeilingVentTrunk", new Vector3(centerX, 3.04f, 0f), new Vector3(9.7f, 0.25f, 0.68f), darkPanel);
            for (int i = -2; i <= 2; i++)
            {
                float x = centerX + i * 1.85f;
                CreateBox(group.transform, "VentGrille", new Vector3(x, 2.89f, 0f), new Vector3(1.10f, 0.035f, 0.48f), steel);

                CreateBox(group.transform, "FloorGrate", new Vector3(x, 0.145f, 0f), new Vector3(1.28f, 0.035f, 1.18f), steel);
                for (int slot = -2; slot <= 2; slot++)
                {
                    CreateBox(group.transform, "GrateSlot", new Vector3(x, 0.168f, slot * 0.19f), new Vector3(1.05f, 0.018f, 0.055f), darkPanel);
                }
            }

            foreach (float end in new[] { -5.48f, 5.48f })
            {
                foreach (float side in new[] { -2.26f, 2.26f })
                {
                    CreateBox(group.transform, "CornerBrace", new Vector3(centerX + end, 1.60f, side), new Vector3(0.18f, 2.55f, 0.16f), steel);
                }
            }
        }

        private void CreateLocomotiveDetails(Transform parent)
        {
            GameObject group = NewGroup(parent, "Locomotive_ProfessionalDetails");

            foreach (float z in new[] { -1.88f, 1.88f })
            {
                CreateBox(group.transform, "AvionicsCabinet", new Vector3(-10.1f, 1.02f, z), new Vector3(1.35f, 1.78f, 0.42f), darkPanel);
                CreateBox(group.transform, "CabinetInset", new Vector3(-10.1f, 1.08f, z - Mathf.Sign(z) * 0.225f), new Vector3(0.96f, 1.22f, 0.035f), steel);
                for (int i = 0; i < 3; i++)
                {
                    CreateBox(group.transform, "CabinetStatus", new Vector3(-10.38f + i * 0.28f, 1.52f, z - Mathf.Sign(z) * 0.248f), new Vector3(0.12f, 0.055f, 0.025f), warmAccent);
                }
            }

            CreateCylinder(group.transform, "DriverFootRail", new Vector3(-15.55f, 0.34f, 0f), new Vector3(0.065f, 1.65f, 0.065f), new Vector3(90f, 0f, 0f), steel);
            CreateCylinder(group.transform, "CabPipeLeft", new Vector3(-14.1f, 2.72f, -1.72f), new Vector3(0.055f, 2.6f, 0.055f), new Vector3(0f, 0f, 90f), pipe);
            CreateCylinder(group.transform, "CabPipeRight", new Vector3(-14.1f, 2.72f, 1.72f), new Vector3(0.055f, 2.6f, 0.055f), new Vector3(0f, 0f, 90f), pipe);
        }

        private void CreateGeneratorDetails(Transform parent)
        {
            GameObject group = NewGroup(parent, "Generator_ProfessionalDetails");

            foreach (float z in new[] { -2.05f, 2.05f })
            {
                for (int p = 0; p < 3; p++)
                {
                    float y = 0.78f + p * 0.42f;
                    CreateCylinder(group.transform, "GeneratorPipe", new Vector3(0f, y, z), new Vector3(0.055f + p * 0.008f, 4.6f, 0.055f + p * 0.008f), new Vector3(0f, 0f, 90f), p == 1 ? warmAccent : pipe);
                }
            }

            foreach (float x in new[] { -3.9f, 3.75f })
            {
                CreateCylinder(group.transform, "PressureTank", new Vector3(x, 0.92f, 1.78f), new Vector3(0.42f, 0.72f, 0.42f), Vector3.zero, darkPanel);
                CreateCylinder(group.transform, "TankCap", new Vector3(x, 1.68f, 1.78f), new Vector3(0.34f, 0.10f, 0.34f), Vector3.zero, steel);
            }

            CreateSafetyRail(group.transform, new Vector3(0f, 0.72f, -0.55f), 6.2f);
            CreateBox(group.transform, "OverheadCableRack", new Vector3(0f, 2.70f, 1.65f), new Vector3(7.4f, 0.10f, 0.48f), cable);
        }

        private void CreateLivingDetails(Transform parent)
        {
            GameObject group = NewGroup(parent, "Living_ProfessionalDetails");

            for (int i = 0; i < 4; i++)
            {
                float x = 8.1f + i * 2.30f;
                CreateBox(group.transform, "PersonalLocker", new Vector3(x, 1.13f, 2.04f), new Vector3(0.72f, 2.02f, 0.42f), livingAccent);
                CreateBox(group.transform, "LockerDoor", new Vector3(x, 1.13f, 1.81f), new Vector3(0.56f, 1.76f, 0.035f), darkPanel);
                CreateBox(group.transform, "LockerHandle", new Vector3(x + 0.20f, 1.12f, 1.78f), new Vector3(0.04f, 0.34f, 0.025f), steel);
            }

            CreateCylinder(group.transform, "HeaterPipeLow", new Vector3(12.4f, 0.52f, -2.14f), new Vector3(0.045f, 4.6f, 0.045f), new Vector3(0f, 0f, 90f), pipe);
            CreateCylinder(group.transform, "HeaterPipeHigh", new Vector3(12.4f, 0.72f, -2.14f), new Vector3(0.045f, 4.6f, 0.045f), new Vector3(0f, 0f, 90f), pipe);

            CreateBox(group.transform, "PrivacyDivider", new Vector3(14.25f, 1.05f, -1.08f), new Vector3(0.08f, 1.65f, 1.55f), darkPanel);
            CreateBox(group.transform, "DividerTrim", new Vector3(14.20f, 1.86f, -1.08f), new Vector3(0.14f, 0.08f, 1.65f), steel);
        }

        private void CreateWorkshopDetails(Transform parent)
        {
            GameObject group = NewGroup(parent, "Workshop_ProfessionalDetails");

            foreach (float x in new[] { 21.9f, 25.0f, 28.0f })
            {
                CreateBox(group.transform, "ToolPegboard", new Vector3(x, 1.78f, -2.18f), new Vector3(2.15f, 1.20f, 0.08f), workshopAccent);
                for (int i = -2; i <= 2; i++)
                {
                    CreateBox(group.transform, "PegboardTool", new Vector3(x + i * 0.31f, 1.78f + ((i & 1) == 0 ? 0.16f : -0.18f), -2.10f), new Vector3(0.10f, 0.46f, 0.045f), steel);
                }
            }

            CreateBox(group.transform, "HoistRail", new Vector3(24.8f, 2.88f, 0.62f), new Vector3(8.6f, 0.18f, 0.20f), steel);
            CreateBox(group.transform, "HoistTrolley", new Vector3(25.7f, 2.70f, 0.62f), new Vector3(0.48f, 0.30f, 0.42f), darkPanel);
            CreateCylinder(group.transform, "HoistCable", new Vector3(25.7f, 2.05f, 0.62f), new Vector3(0.025f, 0.55f, 0.025f), Vector3.zero, cable);
            CreateBox(group.transform, "HoistHook", new Vector3(25.7f, 1.47f, 0.62f), new Vector3(0.18f, 0.22f, 0.12f), warmAccent);

            CreateBox(group.transform, "PartsCabinet", new Vector3(20.4f, 1.02f, 1.93f), new Vector3(1.20f, 1.78f, 0.48f), darkPanel);
            for (int row = 0; row < 4; row++)
            {
                CreateBox(group.transform, "PartsDrawer", new Vector3(20.4f, 0.49f + row * 0.35f, 1.66f), new Vector3(0.96f, 0.24f, 0.05f), workshopAccent);
            }
        }

        private void CreateGreenhouseDetails(Transform parent)
        {
            GameObject group = NewGroup(parent, "Greenhouse_ProfessionalDetails");

            foreach (float z in new[] { -2.02f, 2.02f })
            {
                CreateCylinder(group.transform, "IrrigationMain", new Vector3(37.2f, 0.58f, z), new Vector3(0.050f, 4.8f, 0.050f), new Vector3(0f, 0f, 90f), greenhouseAccent);
                for (int i = -2; i <= 2; i++)
                {
                    float x = 37.2f + i * 1.85f;
                    CreateCylinder(group.transform, "IrrigationDrop", new Vector3(x, 0.84f, z), new Vector3(0.028f, 0.26f, 0.028f), Vector3.zero, pipe);
                }
            }

            CreateCylinder(group.transform, "MistLine", new Vector3(37.2f, 2.72f, 0f), new Vector3(0.045f, 4.8f, 0.045f), new Vector3(0f, 0f, 90f), pipe);
            for (int i = -2; i <= 2; i++)
            {
                float x = 37.2f + i * 1.85f;
                CreateCylinder(group.transform, "MistNozzle", new Vector3(x, 2.54f, 0f), new Vector3(0.035f, 0.13f, 0.035f), Vector3.zero, steel);
            }

            CreateBox(group.transform, "WaterManifold", new Vector3(41.25f, 1.05f, -1.95f), new Vector3(0.95f, 1.50f, 0.42f), darkPanel);
            for (int i = 0; i < 3; i++)
            {
                CreateCylinder(group.transform, "ValveWheel", new Vector3(41.25f, 0.65f + i * 0.40f, -1.70f), new Vector3(0.12f, 0.035f, 0.12f), new Vector3(90f, 0f, 0f), i == 2 ? warmAccent : greenhouseAccent);
            }
        }

        private void CreateConnectorDetails(Transform parent)
        {
            GameObject group = NewGroup(parent, "Connector_ProfessionalDetails");
            float[] connectorCenters = { -6.2f, 6.2f, 18.6f, 31.0f };

            foreach (float x in connectorCenters)
            {
                foreach (float z in new[] { -0.82f, 0.82f })
                {
                    CreateBox(group.transform, "ConnectorBellows", new Vector3(x, 1.32f, z), new Vector3(0.42f, 2.42f, 0.10f), rubber);
                }
                CreateBox(group.transform, "ConnectorBellowsTop", new Vector3(x, 2.49f, 0f), new Vector3(0.42f, 0.10f, 1.74f), rubber);
                CreateBox(group.transform, "ExpansionPlate", new Vector3(x, 0.135f, 0f), new Vector3(0.48f, 0.035f, 1.52f), steel);
                CreateBox(group.transform, "DoorHeader", new Vector3(x, 2.38f, 0f), new Vector3(0.52f, 0.16f, 1.76f), darkPanel);
            }
        }

        private void CreateSafetyRail(Transform parent, Vector3 center, float length)
        {
            CreateCylinder(parent, "SafetyRailTop", center + new Vector3(0f, 0.50f, 0f), new Vector3(0.035f, length * 0.5f, 0.035f), new Vector3(0f, 0f, 90f), steel);
            for (int i = -2; i <= 2; i++)
            {
                CreateCylinder(parent, "SafetyRailPost", center + new Vector3(i * length / 4.4f, 0.12f, 0f), new Vector3(0.035f, 0.42f, 0.035f), Vector3.zero, steel);
            }
        }

        private static GameObject NewGroup(Transform parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group;
        }

        private GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            ConfigureDecorative(go);
            return go;
        }

        private GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Vector3 euler, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            ConfigureDecorative(go);
            return go;
        }

        private static void ConfigureDecorative(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static Material CreateMaterial(string name, Color color, float smoothness, float metallic)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            return material;
        }
    }
}
