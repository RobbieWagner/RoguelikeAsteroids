using System.Collections;
using UnityEngine;

namespace RobbieWagnerGames.UnityExtensions
{
    /// <summary>
    /// Provides extension methods for Unity's Camera component
    /// </summary>
    public static class CameraExtensions
    {
        /// <summary>
        /// Gets a ray from camera through mouse position in screen coordinates
        /// </summary>
        public static Ray GetMouseRay(this Camera camera)
        {
            if (camera == null)
            {
                Debug.LogWarning("Camera is null - returning default ray");
                return new Ray();
            }
            return camera.ScreenPointToRay(Input.mousePosition);
        }

        /// <summary>
        /// Shakes the camera with configurable parameters (coroutine version)
        /// </summary>
        /// <param name="camera">Target camera</param>
        /// <param name="duration">Shake duration in seconds</param>
        /// <param name="magnitude">Shake intensity</param>
        /// <param name="frequency">Shake frequency (higher = more rapid)</param>
        /// <param name="useLocalSpace">Whether to use local space coordinates</param>
        public static IEnumerator Shake(this Camera camera,
            float duration,
            float magnitude,
            float frequency = 25f,
            bool useLocalSpace = true)
        {
            if (camera == null) yield break;

            Transform cameraTransform = camera.transform;
            Vector3 originalPosition = useLocalSpace ? 
                cameraTransform.localPosition : 
                cameraTransform.position;

            float elapsed = 0f;
            float seed = Random.value;

            while (elapsed < duration)
            {
                float percentComplete = elapsed / duration;
                float damper = 1f - Mathf.Clamp01(percentComplete * percentComplete);
                
                float x = Mathf.PerlinNoise(seed, elapsed * frequency) * 2f - 1f;
                float y = Mathf.PerlinNoise(seed + 1f, elapsed * frequency) * 2f - 1f;
                
                x *= magnitude * damper;
                y *= magnitude * damper;

                Vector3 offset = new Vector3(x, y, 0f);
                
                if (useLocalSpace)
                {
                    cameraTransform.localPosition = originalPosition + offset;
                }
                else
                {
                    cameraTransform.position = originalPosition + offset;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (useLocalSpace)
            {
                cameraTransform.localPosition = originalPosition;
            }
            else
            {
                cameraTransform.position = originalPosition;
            }
        }
    }
}