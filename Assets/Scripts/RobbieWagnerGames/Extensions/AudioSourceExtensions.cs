using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace RobbieWagnerGames.UnityExtensions
{
    /// <summary>
    /// Provides extension methods for Unity's AudioSource component
    /// </summary>
    public static class AudioSourceExtensions
    {
        private const float MinVolume = 0f;
        private const float MaxVolume = 1f;

        /// <summary>
        /// Fades an AudioSource's volume in over time (coroutine version)
        /// </summary>
        /// <param name="audioSource">Target AudioSource</param>
        /// <param name="duration">Fade duration in seconds</param>
        /// <param name="targetVolume">Target volume (clamped 0-1)</param>
        /// <param name="useUnscaledTime">Whether to ignore Time.timeScale</param>
        /// <param name="onComplete">Callback when fade completes</param>
        public static IEnumerator FadeIn(this AudioSource audioSource, 
            float duration, 
            float targetVolume = MaxVolume,
            bool useUnscaledTime = true,
            Action onComplete = null)
        {
            if (audioSource == null) yield break;
            if (audioSource.volume >= targetVolume) yield break;

            targetVolume = Mathf.Clamp(targetVolume, MinVolume, MaxVolume);
            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            audioSource.volume = targetVolume;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Fades an AudioSource's volume out over time (coroutine version)
        /// </summary>
        /// <param name="audioSource">Target AudioSource</param>
        /// <param name="duration">Fade duration in seconds</param>
        /// <param name="useUnscaledTime">Whether to ignore Time.timeScale</param>
        /// <param name="onComplete">Callback when fade completes</param>
        public static IEnumerator FadeOut(this AudioSource audioSource, 
            float duration,
            bool useUnscaledTime = true,
            Action onComplete = null)
        {
            if (audioSource == null) yield break;

            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, MinVolume, elapsed / duration);
                yield return null;
            }

            audioSource.volume = MinVolume;
            onComplete?.Invoke();
        }

#if CSHARP_7_3_OR_NEWER
        /// <summary>
        /// Asynchronously fades an AudioSource's volume in
        /// </summary>
        public static async Task FadeInAsync(this AudioSource audioSource,
            float duration,
            float targetVolume = MaxVolume,
            bool useUnscaledTime = true,
            Action onComplete = null)
        {
            if (audioSource == null) return;
            if (audioSource.volume >= targetVolume) return;

            targetVolume = Mathf.Clamp(targetVolume, MinVolume, MaxVolume);
            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                await Task.Yield();
            }

            audioSource.volume = targetVolume;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Asynchronously fades an AudioSource's volume out
        /// </summary>
        public static async Task FadeOutAsync(this AudioSource audioSource,
            float duration,
            bool useUnscaledTime = true,
            Action onComplete = null)
        {
            if (audioSource == null) return;

            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, MinVolume, elapsed / duration);
                await Task.Yield();
            }

            audioSource.volume = MinVolume;
            onComplete?.Invoke();
        }
#endif
    }
}