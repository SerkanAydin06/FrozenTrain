using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FrozenTrain.Atmosphere
{
    public static class UltraCameraPostEnable
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Enable()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = GameObject.Find("PlayerCamera");
                if (cameraObject != null)
                {
                    camera = cameraObject.GetComponent<Camera>();
                }
            }

            if (camera == null)
            {
                return;
            }

            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.dithering = true;
        }
    }
}
