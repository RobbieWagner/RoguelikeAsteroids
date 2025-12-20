using System.Collections;
using RobbieWagnerGames.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RobbieWagnerGames.Utilities
{
    public class SceneLoadManager : MonoBehaviourSingleton<SceneLoadManager>
    {
        public IEnumerator LoadSceneAdditive(string sceneName, System.Action callback = null)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return new WaitForSeconds(0.5f); // Small delay to ensure scene loads properly
            // Execute callback if provided
            callback?.Invoke();
            
            yield return StartCoroutine(ScreenCover.Instance.FadeCoverOut());
        }

        public IEnumerator UnloadScene(string sceneName, bool coverScreen = true, System.Action callback = null)
        {
            if (coverScreen)
                yield return StartCoroutine(ScreenCover.Instance.FadeCoverIn());
            SceneManager.UnloadSceneAsync(sceneName);
            yield return new WaitForSeconds(0.5f); // Small delay to ensure scene loads properly
            // Execute callback if provided
            callback?.Invoke();
        }
    }
}