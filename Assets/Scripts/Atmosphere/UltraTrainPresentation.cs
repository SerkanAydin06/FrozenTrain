using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FrozenTrain.Atmosphere
{
    /// <summary>
    /// One-shot presentation layer for the locked five-car prototype.
    /// It adds post processing, weathering, frost, car-specific hero details,
    /// exterior snow and lighting identity without changing gameplay or car count.
    /// </summary>
    public sealed class UltraTrainPresentation : MonoBehaviour
    {
        private const string RootName = "ULTRA_PRESENTATION";

        private Material blackMetal;
        private Material brushedSteel;
        private Material grime;
        private Material frost;
        private Material rubber;
        private Material copper;
        private Material amberGlow;
        private Material redGlow;
        private Material cyanGlow;
        private Material fabric;
        private Material livingBlue;
        private Material workshopOchre;
        private Material greenhouseGreen;
        private Material plantDark;
        private Material snowParticleMaterial;
        private VolumeProfile runtimeVolumeProfile;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            GameObject train = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (train == null || train.GetComponent<UltraTrainPresentation>() != null)
            {
                return;
            }

            train.AddComponent<UltraTrainPresentation>();
        }

        private void Awake()
        {
            if (transform.Find(RootName) != null)
            {
                return;
            }

            CreateMaterials();

            GameObject rootObject = new GameObject(RootName);
            rootObject.transform.SetParent(transform, false);
            Transform root = rootObject.transform;

            ApplyPostProcessing(root);
            RetuneLighting();
            WeatherExistingMaterials();
            AddCommonWearAndTrim(root);
            AddFrostToWindows();
            AddConnectorPolish(root);
            AddLocomotiveHeroPass(root);
            AddGeneratorHeroPass(root);
            AddLivingHeroPass(root);
            AddWorkshopHeroPass(root);
            AddGreenhouseHeroPass(root);
            AddExteriorSnow(root);
        }

        private void CreateMaterials()
        {
            blackMetal = Lit("Ultra_BlackMetal", new Color(0.025f, 0.030f, 0.036f), 0.38f, 0.82f);
            brushedSteel = Lit("Ultra_BrushedSteel", new Color(0.25f, 0.29f, 0.32f), 0.46f, 0.86f);
            grime = Lit("Ultra_Grime", new Color(0.018f, 0.021f, 0.024f), 0.08f, 0.05f);
            rubber = Lit("Ultra_Rubber", new Color(0.018f, 0.020f, 0.023f), 0.10f, 0.02f);
            copper = Lit("Ultra_Copper", new Color(0.32f, 0.115f, 0.045f), 0.36f, 0.72f);
            fabric = Lit("Ultra_Fabric", new Color(0.12f, 0.13f, 0.14f), 0.06f, 0.0f);
            livingBlue = Lit("Ultra_LivingBlue", new Color(0.07f, 0.19f, 0.23f), 0.20f, 0.12f);
            workshopOchre = Lit("Ultra_WorkshopOchre", new Color(0.26f, 0.17f, 0.055f), 0.22f, 0.35f);
            greenhouseGreen = Lit("Ultra_GreenhouseGreen", new Color(0.045f, 0.24f, 0.105f), 0.18f, 0.06f);
            plantDark = Lit("Ultra_PlantDark", new Color(0.025f, 0.13f, 0.050f), 0.10f, 0.0f);

            frost = Transparent("Ultra_Frost", new Color(0.78f, 0.90f, 0.96f, 0.42f), 0.72f, 0.02f);
            amberGlow = Emissive("Ultra_AmberGlow", new Color(0.34f, 0.10f, 0.015f), new Color(2.4f, 0.52f, 0.05f));
            redGlow = Emissive("Ultra_RedGlow", new Color(0.28f, 0.025f, 0.018f), new Color(2.7f, 0.08f, 0.035f));
            cyanGlow = Emissive("Ultra_CyanGlow", new Color(0.018f, 0.16f, 0.21f), new Color(0.05f, 1.15f, 1.7f));
        }

        private void ApplyPostProcessing(Transform root)
        {
            GameObject post = new GameObject("CinematicPostProcessing");
            post.transform.SetParent(root, false);

            Volume volume = post.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 80f;

            runtimeVolumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            runtimeVolumeProfile.hideFlags = HideFlags.DontSave;
            volume.sharedProfile = runtimeVolumeProfile;

            Tonemapping tonemapping = runtimeVolumeProfile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            Bloom bloom = runtimeVolumeProfile.Add<Bloom>(true);
            bloom.intensity.Override(0.20f);
            bloom.threshold.Override(1.05f);
            bloom.scatter.Override(0.54f);

            ColorAdjustments color = runtimeVolumeProfile.Add<ColorAdjustments>(true);
            color.postExposure.Override(-0.08f);
            color.contrast.Override(10f);
            color.saturation.Override(-9f);

            WhiteBalance white = runtimeVolumeProfile.Add<WhiteBalance>(true);
            white.temperature.Override(-7f);
            white.tint.Override(-2f);

            Vignette vignette = runtimeVolumeProfile.Add<Vignette>(true);
            vignette.intensity.Override(0.16f);
            vignette.smoothness.Override(0.62f);

            FilmGrain grain = runtimeVolumeProfile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.065f);
            grain.response.Override(0.72f);
        }

        private void RetuneLighting()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light == null)
                {
                    continue;
                }

                if (light.name == "ColdExteriorLight")
                {
                    light.intensity = 0.58f;
                    light.color = new Color(0.58f, 0.70f, 0.92f);
                    continue;
                }

                if (light.name != "InteriorLight")
                {
                    continue;
                }

                float x = light.transform.position.x;
                light.shadows = LightShadows.None;
                light.range = 7.0f;

                if (x < -6.2f)
                {
                    light.color = new Color(0.82f, 0.90f, 1.0f);
                    light.intensity = 1.22f;
                }
                else if (x < 6.2f)
                {
                    light.color = new Color(1.0f, 0.55f, 0.27f);
                    light.intensity = 1.55f;
                }
                else if (x < 18.6f)
                {
                    light.color = new Color(1.0f, 0.72f, 0.48f);
                    light.intensity = 1.42f;
                }
                else if (x < 31.0f)
                {
                    light.color = new Color(0.95f, 0.79f, 0.60f);
                    light.intensity = 1.38f;
                }
                else
                {
                    light.color = new Color(0.72f, 0.94f, 0.72f);
                    light.intensity = 1.48f;
                }
            }
        }

        private void WeatherExistingMaterials()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.sharedMaterial == null)
                {
                    continue;
                }

                string objectName = renderer.gameObject.name;
                if (objectName.Contains("Glass") || objectName.Contains("Screen") || objectName.Contains("Glow") || objectName.Contains("Indicator"))
                {
                    continue;
                }

                Material source = renderer.sharedMaterial;
                if (!source.HasProperty("_BaseColor"))
                {
                    continue;
                }

                Color baseColor = source.GetColor("_BaseColor");
                float noise = StableNoise(renderer.transform.position);
                float dirt = Mathf.Lerp(0.84f, 0.98f, noise);

                if (objectName.Contains("Floor") || objectName.Contains("Aisle") || objectName.Contains("Rail"))
                {
                    dirt *= 0.88f;
                }

                block.Clear();
                block.SetColor("_BaseColor", new Color(baseColor.r * dirt, baseColor.g * dirt, baseColor.b * dirt, baseColor.a));

                if (source.HasProperty("_Smoothness"))
                {
                    block.SetFloat("_Smoothness", Mathf.Lerp(0.13f, 0.38f, noise));
                }

                if (source.HasProperty("_Metallic"))
                {
                    float sourceMetal = source.GetFloat("_Metallic");
                    block.SetFloat("_Metallic", Mathf.Clamp01(sourceMetal + (noise - 0.5f) * 0.12f));
                }

                renderer.SetPropertyBlock(block);
            }
        }

        private void AddCommonWearAndTrim(Transform root)
        {
            float[] centers = { -12.4f, 0f, 12.4f, 24.8f, 37.2f };
            for (int car = 0; car < centers.Length; car++)
            {
                float center = centers[car];
                GameObject group = Group(root, $"Car_{car + 1:00}_UltraTrim");

                foreach (float side in new[] { -1f, 1f })
                {
                    CreateBox(group.transform, "FloorGrimeSeam", new Vector3(center, 0.245f, side * 2.31f), new Vector3(10.9f, 0.045f, 0.09f), grime);
                    CreateBox(group.transform, "UpperWearRail", new Vector3(center, 2.61f, side * 2.30f), new Vector3(10.55f, 0.055f, 0.075f), brushedSteel);

                    for (int i = -4; i <= 4; i++)
                    {
                        float x = center + i * 1.15f;
                        CreateBox(group.transform, "PanelFastener", new Vector3(x, 1.17f, side * 2.305f), new Vector3(0.045f, 0.045f, 0.035f), brushedSteel);
                    }
                }

                for (int i = -4; i <= 4; i++)
                {
                    float x = center + i * 1.18f;
                    CreateBox(group.transform, "AisleWearStrip", new Vector3(x, 0.184f, 0f), new Vector3(0.55f, 0.012f, 1.12f), i % 2 == 0 ? blackMetal : grime);
                }

                CreateBox(group.transform, "CeilingSpine", new Vector3(center, 3.06f, 0f), new Vector3(10.3f, 0.08f, 0.13f), brushedSteel);
            }
        }

        private void AddFrostToWindows()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (renderer.gameObject.name == "WindowGlass")
                {
                    AddSideWindowFrost(renderer.transform);
                }
                else if (renderer.gameObject.name == "CabWindshield")
                {
                    AddWindshieldFrost(renderer.transform);
                }
            }
        }

        private void AddSideWindowFrost(Transform pane)
        {
            if (pane.Find("UltraFrost") != null)
            {
                return;
            }

            GameObject root = Group(pane, "UltraFrost");
            CreateLocalBox(root.transform, "FrostTop", new Vector3(0f, 0.45f, -0.62f), new Vector3(0.94f, 0.10f, 0.35f), frost);
            CreateLocalBox(root.transform, "FrostBottom", new Vector3(0f, -0.45f, -0.62f), new Vector3(0.94f, 0.10f, 0.35f), frost);
            CreateLocalBox(root.transform, "FrostLeft", new Vector3(-0.46f, 0f, -0.62f), new Vector3(0.08f, 0.78f, 0.35f), frost);
            CreateLocalBox(root.transform, "FrostRight", new Vector3(0.46f, 0f, -0.62f), new Vector3(0.08f, 0.78f, 0.35f), frost);

            for (int i = -2; i <= 2; i++)
            {
                float x = i * 0.17f;
                float height = 0.15f + (i + 3) * 0.025f;
                CreateLocalBox(root.transform, "CondensationStreak", new Vector3(x, 0.26f - height * 0.5f, -0.64f), new Vector3(0.018f, height, 0.28f), frost);
            }
        }

        private void AddWindshieldFrost(Transform pane)
        {
            if (pane.Find("UltraFrost") != null)
            {
                return;
            }

            GameObject root = Group(pane, "UltraFrost");
            CreateLocalBox(root.transform, "FrostTop", new Vector3(-0.62f, 0.44f, 0f), new Vector3(0.34f, 0.09f, 0.94f), frost);
            CreateLocalBox(root.transform, "FrostBottom", new Vector3(-0.62f, -0.44f, 0f), new Vector3(0.34f, 0.09f, 0.94f), frost);
            CreateLocalBox(root.transform, "FrostLeft", new Vector3(-0.62f, 0f, -0.46f), new Vector3(0.34f, 0.78f, 0.08f), frost);
            CreateLocalBox(root.transform, "FrostRight", new Vector3(-0.62f, 0f, 0.46f), new Vector3(0.34f, 0.78f, 0.08f), frost);
        }

        private void AddConnectorPolish(Transform root)
        {
            foreach (float x in new[] { -6.2f, 6.2f, 18.6f, 31.0f })
            {
                GameObject group = Group(root, "ConnectorUltraDetail");

                CreateBox(group.transform, "ThresholdPlate", new Vector3(x, 0.205f, 0f), new Vector3(0.82f, 0.055f, 1.48f), brushedSteel);
                for (int stripe = -3; stripe <= 3; stripe++)
                {
                    CreateBox(group.transform, "ThresholdStripe", new Vector3(x + stripe * 0.10f, 0.238f, 0f), new Vector3(0.055f, 0.010f, 1.35f), stripe % 2 == 0 ? amberGlow : blackMetal);
                }

                CreateBox(group.transform, "DoorHeader", new Vector3(x, 2.52f, 0f), new Vector3(0.40f, 0.22f, 1.78f), blackMetal);
                CreateBox(group.transform, "DoorStatusLeft", new Vector3(x - 0.22f, 2.52f, -0.43f), new Vector3(0.055f, 0.10f, 0.34f), redGlow);
                CreateBox(group.transform, "DoorStatusRight", new Vector3(x - 0.22f, 2.52f, 0.43f), new Vector3(0.055f, 0.10f, 0.34f), amberGlow);
            }
        }

        private void AddLocomotiveHeroPass(Transform root)
        {
            GameObject group = Group(root, "Locomotive_UltraHero");

            CreateBox(group.transform, "DashboardBrow", new Vector3(-16.02f, 1.78f, 0f), new Vector3(0.20f, 0.16f, 3.42f), blackMetal);
            CreateBox(group.transform, "ConsoleWearEdge", new Vector3(-15.98f, 0.83f, 0f), new Vector3(0.10f, 0.08f, 3.10f), brushedSteel);

            for (int i = -3; i <= 3; i++)
            {
                float z = i * 0.40f;
                CreateBox(group.transform, "CabIndicator", new Vector3(-15.91f, 1.67f, z), new Vector3(0.035f, 0.055f, 0.16f), i % 3 == 0 ? redGlow : cyanGlow);
            }

            for (int i = 0; i < 3; i++)
            {
                float z = -0.58f + i * 0.58f;
                CreateCylinder(group.transform, "ThrottleLever", new Vector3(-15.25f, 1.02f, z), new Vector3(0.035f, 0.30f, 0.035f), new Vector3(0f, 0f, -20f), brushedSteel);
                CreateBox(group.transform, "LeverGrip", new Vector3(-15.15f, 1.30f, z), new Vector3(0.16f, 0.10f, 0.15f), rubber);
            }

            CreateCylinder(group.transform, "CabHandrailLeft", new Vector3(-12.3f, 1.18f, -2.05f), new Vector3(0.035f, 1.85f, 0.035f), new Vector3(0f, 0f, 90f), brushedSteel);
            CreateCylinder(group.transform, "CabHandrailRight", new Vector3(-12.3f, 1.18f, 2.05f), new Vector3(0.035f, 1.85f, 0.035f), new Vector3(0f, 0f, 90f), brushedSteel);
        }

        private void AddGeneratorHeroPass(Transform root)
        {
            GameObject group = Group(root, "Generator_UltraHero");

            foreach (float z in new[] { -1.72f, 1.72f })
            {
                for (int p = 0; p < 4; p++)
                {
                    float y = 0.50f + p * 0.38f;
                    CreateCylinder(group.transform, "HighPressurePipe", new Vector3(0.45f, y, z), new Vector3(0.045f + p * 0.006f, 4.45f, 0.045f + p * 0.006f), new Vector3(0f, 0f, 90f), p == 2 ? copper : brushedSteel);
                }
            }

            foreach (float x in new[] { -3.25f, 3.25f })
            {
                CreateCylinder(group.transform, "ValveBody", new Vector3(x, 1.25f, -1.62f), new Vector3(0.20f, 0.12f, 0.20f), new Vector3(0f, 0f, 90f), blackMetal);
                CreateCylinder(group.transform, "ValveWheel", new Vector3(x - 0.14f, 1.25f, -1.62f), new Vector3(0.24f, 0.035f, 0.24f), new Vector3(0f, 0f, 90f), redGlow);
            }

            CreateBox(group.transform, "GeneratorGuardTop", new Vector3(0f, 1.92f, 1.30f), new Vector3(5.9f, 0.09f, 0.11f), brushedSteel);
            for (int i = -3; i <= 3; i++)
            {
                CreateCylinder(group.transform, "GeneratorGuardPost", new Vector3(i * 0.85f, 1.22f, 1.30f), new Vector3(0.035f, 0.68f, 0.035f), Vector3.zero, brushedSteel);
            }
        }

        private void AddLivingHeroPass(Transform root)
        {
            GameObject group = Group(root, "Living_UltraHero");

            foreach (float z in new[] { -1.58f, 1.58f })
            {
                CreateBox(group.transform, "OverheadRack", new Vector3(12.5f, 2.58f, z), new Vector3(7.25f, 0.10f, 0.62f), brushedSteel);
                for (int i = -3; i <= 3; i++)
                {
                    CreateBox(group.transform, "RackBracket", new Vector3(12.5f + i * 1.05f, 2.34f, z), new Vector3(0.06f, 0.46f, 0.08f), blackMetal);
                }
            }

            CreateBox(group.transform, "BenchLeft", new Vector3(15.4f, 0.48f, -1.05f), new Vector3(1.55f, 0.45f, 0.58f), livingBlue);
            CreateBox(group.transform, "BenchRight", new Vector3(15.4f, 0.48f, 1.05f), new Vector3(1.55f, 0.45f, 0.58f), livingBlue);
            CreateBox(group.transform, "BenchBackLeft", new Vector3(15.4f, 0.92f, -1.32f), new Vector3(1.55f, 0.55f, 0.12f), fabric);
            CreateBox(group.transform, "BenchBackRight", new Vector3(15.4f, 0.92f, 1.32f), new Vector3(1.55f, 0.55f, 0.12f), fabric);

            for (int i = 0; i < 3; i++)
            {
                float x = 9.4f + i * 2.0f;
                CreateBox(group.transform, "CurtainPanel", new Vector3(x, 1.55f, -1.16f), new Vector3(0.055f, 1.65f, 0.86f), fabric);
            }
        }

        private void AddWorkshopHeroPass(Transform root)
        {
            GameObject group = Group(root, "Workshop_UltraHero");

            foreach (float x in new[] { 22.0f, 27.0f })
            {
                CreateCylinder(group.transform, "GasCylinder", new Vector3(x, 0.66f, 1.93f), new Vector3(0.27f, 0.58f, 0.27f), Vector3.zero, blackMetal);
                CreateCylinder(group.transform, "GasCylinderCap", new Vector3(x, 1.29f, 1.93f), new Vector3(0.16f, 0.08f, 0.16f), Vector3.zero, brushedSteel);
                CreateBox(group.transform, "GasLabel", new Vector3(x, 0.78f, 1.64f), new Vector3(0.24f, 0.28f, 0.025f), x < 25f ? redGlow : workshopOchre);
            }

            CreateBox(group.transform, "WeldingScreen", new Vector3(25.7f, 1.40f, -1.15f), new Vector3(0.10f, 2.15f, 1.55f), workshopOchre);
            CreateBox(group.transform, "WorkbenchLip", new Vector3(22.0f, 1.58f, -1.72f), new Vector3(3.15f, 0.09f, 0.82f), brushedSteel);
            CreateBox(group.transform, "WorkbenchLip", new Vector3(27.2f, 1.58f, 1.72f), new Vector3(3.15f, 0.09f, 0.82f), brushedSteel);

            for (int i = -2; i <= 2; i++)
            {
                CreateCylinder(group.transform, "HangingCable", new Vector3(24.8f + i * 0.72f, 2.30f, -1.75f), new Vector3(0.018f, 0.47f + Mathf.Abs(i) * 0.08f, 0.018f), Vector3.zero, rubber);
            }
        }

        private void AddGreenhouseHeroPass(Transform root)
        {
            GameObject group = Group(root, "Greenhouse_UltraHero");

            for (int bed = 0; bed < 4; bed++)
            {
                float x = 33.5f + bed * 2.3f;
                for (int plant = -2; plant <= 2; plant++)
                {
                    float z = -1.35f + plant * 0.18f;
                    CreateBox(group.transform, "PlantStem", new Vector3(x, 1.33f, z), new Vector3(0.045f, 0.52f, 0.045f), plantDark);
                    CreateBox(group.transform, "PlantLeaf", new Vector3(x - 0.12f, 1.45f, z), new Vector3(0.30f, 0.08f, 0.16f), greenhouseGreen, new Vector3(0f, 0f, 22f));
                    CreateBox(group.transform, "PlantLeaf", new Vector3(x + 0.12f, 1.60f, z), new Vector3(0.30f, 0.08f, 0.16f), greenhouseGreen, new Vector3(0f, 0f, -22f));
                }
            }

            foreach (float z in new[] { -1.92f, 1.92f })
            {
                CreateCylinder(group.transform, "MistPipe", new Vector3(37.2f, 2.72f, z), new Vector3(0.035f, 4.7f, 0.035f), new Vector3(0f, 0f, 90f), brushedSteel);
                for (int i = -4; i <= 4; i++)
                {
                    float x = 37.2f + i * 1.0f;
                    CreateCylinder(group.transform, "MistNozzle", new Vector3(x, 2.58f, z), new Vector3(0.018f, 0.13f, 0.018f), Vector3.zero, copper);
                }
            }

            CreateBox(group.transform, "WaterSystemPanel", new Vector3(40.1f, 1.55f, 2.08f), new Vector3(1.35f, 1.25f, 0.16f), blackMetal);
            CreateBox(group.transform, "WaterSystemScreen", new Vector3(40.1f, 1.68f, 1.98f), new Vector3(0.84f, 0.42f, 0.025f), cyanGlow);
        }

        private void AddExteriorSnow(Transform root)
        {
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader == null)
            {
                return;
            }

            snowParticleMaterial = new Material(particleShader)
            {
                name = "Ultra_SnowParticle",
                hideFlags = HideFlags.DontSave
            };
            if (snowParticleMaterial.HasProperty("_BaseColor"))
            {
                snowParticleMaterial.SetColor("_BaseColor", new Color(0.90f, 0.96f, 1f, 0.78f));
            }

            GameObject snow = new GameObject("WindowSnowStorm");
            snow.transform.SetParent(root, false);
            snow.transform.position = new Vector3(12f, 4.5f, 0f);

            ParticleSystem particles = snow.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.maxParticles = 520;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4.5f, 7.0f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.085f);
            main.startColor = new Color(0.92f, 0.97f, 1f, 0.82f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 72f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(98f, 9f, 18f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-13.5f, -9.5f);
            velocity.y = new ParticleSystem.MinMaxCurve(-2.0f, -0.8f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.45f;
            noise.frequency = 0.22f;
            noise.scrollSpeed = 0.18f;

            ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.material = snowParticleMaterial;
            particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
            particleRenderer.receiveShadows = false;

            particles.Play();
        }

        private static float StableNoise(Vector3 p)
        {
            float value = Mathf.Sin(p.x * 12.9898f + p.y * 78.233f + p.z * 37.719f) * 43758.5453f;
            return value - Mathf.Floor(value);
        }

        private Material Lit(string name, Color color, float smoothness, float metallic)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = name,
                hideFlags = HideFlags.DontSave
            };
            SetBase(material, color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            return material;
        }

        private Material Emissive(string name, Color baseColor, Color emission)
        {
            Material material = Lit(name, baseColor, 0.38f, 0.18f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            return material;
        }

        private Material Transparent(string name, Color color, float smoothness, float metallic)
        {
            Material material = Lit(name, color, smoothness, metallic);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            SetBase(material, color);
            return material;
        }

        private static void SetBase(Material material, Color color)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
        }

        private static GameObject Group(Transform parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group;
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            return CreateBox(parent, name, position, scale, material, Vector3.zero);
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Vector3 rotation)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localRotation = Quaternion.Euler(rotation);
            box.transform.localScale = scale;
            Renderer renderer = box.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Collider collider = box.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return box;
        }

        private static GameObject CreateLocalBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localRotation = Quaternion.identity;
            box.transform.localScale = localScale;
            Renderer renderer = box.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Collider collider = box.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return box;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Vector3 rotation, Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = position;
            cylinder.transform.localRotation = Quaternion.Euler(rotation);
            cylinder.transform.localScale = scale;
            Renderer renderer = cylinder.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Collider collider = cylinder.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return cylinder;
        }

        private void OnDestroy()
        {
            if (runtimeVolumeProfile != null)
            {
                Destroy(runtimeVolumeProfile);
            }

            if (snowParticleMaterial != null)
            {
                Destroy(snowParticleMaterial);
            }

            DestroyMaterial(blackMetal);
            DestroyMaterial(brushedSteel);
            DestroyMaterial(grime);
            DestroyMaterial(frost);
            DestroyMaterial(rubber);
            DestroyMaterial(copper);
            DestroyMaterial(amberGlow);
            DestroyMaterial(redGlow);
            DestroyMaterial(cyanGlow);
            DestroyMaterial(fabric);
            DestroyMaterial(livingBlue);
            DestroyMaterial(workshopOchre);
            DestroyMaterial(greenhouseGreen);
            DestroyMaterial(plantDark);
        }

        private static void DestroyMaterial(Material material)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}
