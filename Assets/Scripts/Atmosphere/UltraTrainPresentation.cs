using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FrozenTrain.Atmosphere
{
    /// <summary>
    /// Presentation-only layer. It never adds interior props or floor geometry.
    /// Interior layout belongs exclusively to the editor builder.
    /// </summary>
    public sealed class UltraTrainPresentation : MonoBehaviour
    {
        private const string RootName = "ATMOSPHERE_ONLY";
        private VolumeProfile volumeProfile;
        private Material frostMaterial;
        private Material snowMaterial;

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

            GameObject rootObject = new GameObject(RootName);
            rootObject.transform.SetParent(transform, false);

            CreateMaterials();
            CreatePostProcessing(rootObject.transform);
            AddWindowFrost();
            CreateExteriorSnow(rootObject.transform);
        }

        private void CreateMaterials()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit != null)
            {
                frostMaterial = new Material(lit) { name = "Runtime_Frost" };
                Color frost = new Color(0.78f, 0.90f, 0.96f, 0.30f);
                if (frostMaterial.HasProperty("_BaseColor")) frostMaterial.SetColor("_BaseColor", frost);
                if (frostMaterial.HasProperty("_Surface")) frostMaterial.SetFloat("_Surface", 1f);
                if (frostMaterial.HasProperty("_Blend")) frostMaterial.SetFloat("_Blend", 0f);
                if (frostMaterial.HasProperty("_SrcBlend")) frostMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                if (frostMaterial.HasProperty("_DstBlend")) frostMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                if (frostMaterial.HasProperty("_ZWrite")) frostMaterial.SetFloat("_ZWrite", 0f);
                if (frostMaterial.HasProperty("_Smoothness")) frostMaterial.SetFloat("_Smoothness", 0.72f);
                frostMaterial.SetOverrideTag("RenderType", "Transparent");
                frostMaterial.renderQueue = (int)RenderQueue.Transparent;
                frostMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader != null)
            {
                snowMaterial = new Material(particleShader) { name = "Runtime_SnowParticles" };
                if (snowMaterial.HasProperty("_BaseColor")) snowMaterial.SetColor("_BaseColor", new Color(0.92f, 0.96f, 1f, 0.72f));
            }
        }

        private void CreatePostProcessing(Transform parent)
        {
            GameObject postObject = new GameObject("GlobalPostProcessing");
            postObject.transform.SetParent(parent, false);

            Volume volume = postObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50f;

            volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            volumeProfile.hideFlags = HideFlags.DontSave;
            volume.sharedProfile = volumeProfile;

            Tonemapping tonemapping = volumeProfile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            Bloom bloom = volumeProfile.Add<Bloom>(true);
            bloom.intensity.Override(0.13f);
            bloom.threshold.Override(1.10f);
            bloom.scatter.Override(0.48f);

            ColorAdjustments color = volumeProfile.Add<ColorAdjustments>(true);
            color.postExposure.Override(-0.05f);
            color.contrast.Override(7f);
            color.saturation.Override(-5f);

            WhiteBalance whiteBalance = volumeProfile.Add<WhiteBalance>(true);
            whiteBalance.temperature.Override(-5f);

            Vignette vignette = volumeProfile.Add<Vignette>(true);
            vignette.intensity.Override(0.11f);
            vignette.smoothness.Override(0.58f);
        }

        private void AddWindowFrost()
        {
            if (frostMaterial == null)
            {
                return;
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || (renderer.name != "WindowGlass" && renderer.name != "CabWindshield"))
                {
                    continue;
                }

                Transform pane = renderer.transform;
                if (pane.Find("FrostEdges") != null)
                {
                    continue;
                }

                GameObject frostRoot = new GameObject("FrostEdges");
                frostRoot.transform.SetParent(pane, false);

                if (renderer.name == "WindowGlass")
                {
                    CreateFrostStrip(frostRoot.transform, new Vector3(0f, 0.44f, -0.58f), new Vector3(0.96f, 0.10f, 0.26f));
                    CreateFrostStrip(frostRoot.transform, new Vector3(0f, -0.44f, -0.58f), new Vector3(0.96f, 0.10f, 0.26f));
                    CreateFrostStrip(frostRoot.transform, new Vector3(-0.47f, 0f, -0.58f), new Vector3(0.07f, 0.78f, 0.26f));
                    CreateFrostStrip(frostRoot.transform, new Vector3(0.47f, 0f, -0.58f), new Vector3(0.07f, 0.78f, 0.26f));
                }
                else
                {
                    CreateFrostStrip(frostRoot.transform, new Vector3(-0.58f, 0.43f, 0f), new Vector3(0.26f, 0.10f, 0.95f));
                    CreateFrostStrip(frostRoot.transform, new Vector3(-0.58f, -0.43f, 0f), new Vector3(0.26f, 0.10f, 0.95f));
                }
            }
        }

        private void CreateFrostStrip(Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "Frost";
            strip.transform.SetParent(parent, false);
            strip.transform.localPosition = localPosition;
            strip.transform.localScale = localScale;
            strip.GetComponent<Renderer>().sharedMaterial = frostMaterial;
            Collider collider = strip.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private void CreateExteriorSnow(Transform parent)
        {
            if (snowMaterial == null)
            {
                return;
            }

            GameObject snowObject = new GameObject("ExteriorSnow");
            snowObject.transform.SetParent(parent, false);
            snowObject.transform.position = new Vector3(0f, 7f, 0f);

            ParticleSystem particles = snowObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = 5.5f;
            main.startSpeed = 11f;
            main.startSize = 0.075f;
            main.maxParticles = 850;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.05f;

            var emission = particles.emission;
            emission.rateOverTime = 90f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(95f, 8f, 28f);

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = -10f;
            velocity.y = -1.6f;
            velocity.z = 0.6f;

            ParticleSystemRenderer renderer = snowObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = snowMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private void OnDestroy()
        {
            if (volumeProfile != null) Destroy(volumeProfile);
            if (frostMaterial != null) Destroy(frostMaterial);
            if (snowMaterial != null) Destroy(snowMaterial);
        }
    }
}
