using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobbieWagnerGames.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RobbieWagnerGames.Utilities
{
    public class SceneLoadManager : MonoBehaviourSingleton<SceneLoadManager>
    {
        private List<string> loadedScenes = new List<string>();

        public IEnumerator LoadSceneAdditive(string sceneName, bool coverScreen = true, Action callback = null)
        {
            if (coverScreen)
                yield return StartCoroutine(ScreenCover.Instance.FadeCoverIn());
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            while (!asyncLoad.isDone)
                yield return null;
            
            if (!loadedScenes.Contains(sceneName))
                loadedScenes.Add(sceneName);
            
            callback?.Invoke();
            
            if (coverScreen)
                yield return StartCoroutine(ScreenCover.Instance.FadeCoverOut());
        }

        public IEnumerator UnloadScene(string sceneName, bool coverScreen = true, Action callback = null, bool errorOnFail = true)
        {
            if (!IsSceneLoaded(sceneName))
            {
                if (errorOnFail)
                    throw new ArgumentException($"Scene '{sceneName}' is not loaded.");
                else
                {
                    callback?.Invoke();
                    yield break;
                }
            }

            if (coverScreen)
                yield return StartCoroutine(ScreenCover.Instance.FadeCoverIn());
            
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
            
            while (!asyncUnload.isDone)
                yield return null;

            loadedScenes.Remove(sceneName);
            
            callback?.Invoke();
            
            if (coverScreen)
                yield return StartCoroutine(ScreenCover.Instance.FadeCoverOut());
        }

        public IEnumerator UnloadScenes(List<string> sceneNames, bool coverScreen = true, Action callback = null, bool errorOnFail = true)
        {
            List<string> scenesToUnload = sceneNames.Where(IsSceneLoaded).ToList();

            if (!scenesToUnload.Any())
            {
                if (errorOnFail)
                    throw new ArgumentException("None of the listed scenes are loaded.");
                else
                {
                    callback?.Invoke();
                    yield break;
                }
            }

            if (coverScreen)
                yield return StartCoroutine(ScreenCover.Instance.FadeCoverIn());
            
            List<AsyncOperation> unloadOperations = new List<AsyncOperation>();
            
            foreach (string sceneName in scenesToUnload)
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
                unloadOperations.Add(asyncUnload);
            }
            
            foreach (AsyncOperation op in unloadOperations)
            {
                while (!op.isDone)
                    yield return null;
            }
            
            foreach (string sceneName in scenesToUnload)
                loadedScenes.Remove(sceneName);
            
            callback?.Invoke();
            
            if (coverScreen)
                yield return StartCoroutine(ScreenCover.Instance.FadeCoverOut());
        }
        
        public IEnumerator UnloadAllScenesExcept(params string[] scenesToKeep)
        {
            yield return StartCoroutine(ScreenCover.Instance.FadeCoverIn());
            
            List<string> scenesToUnload = loadedScenes
                .Where(scene => !scenesToKeep.Contains(scene))
                .ToList();
            
            if (scenesToUnload.Count > 0)
            {
                List<AsyncOperation> unloadOperations = new List<AsyncOperation>();
                
                foreach (string sceneName in scenesToUnload)
                {
                    AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
                    unloadOperations.Add(asyncUnload);
                }
                
                foreach (AsyncOperation op in unloadOperations)
                {
                    while (!op.isDone)
                        yield return null;
                }
                
                foreach (string sceneName in scenesToUnload)
                    loadedScenes.Remove(sceneName);
            }
            
            yield return StartCoroutine(ScreenCover.Instance.FadeCoverOut());
        }

        private bool IsSceneLoaded(string sceneName)
        {
            if (GetSceneBuildIndex(sceneName) == -1)
                return false;
            
            if (loadedScenes.Contains(sceneName))
                return true;
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded)
                {
                    if (!loadedScenes.Contains(sceneName))
                        loadedScenes.Add(sceneName);
                    return true;
                }
            }
            
            return false;
        }
        
        private int GetSceneBuildIndex(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (name == sceneName)
                    return i;
            }
            return -1;
        }
        
        public List<string> GetLoadedScenes()
        {
            return new List<string>(loadedScenes);
        }
        
        public bool HasSceneLoaded(string sceneName)
        {
            return IsSceneLoaded(sceneName);
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            RefreshLoadedScenes();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        
        private void RefreshLoadedScenes()
        {
            loadedScenes.Clear();
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    loadedScenes.Add(scene.name);
                }
            }
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!loadedScenes.Contains(scene.name))
                loadedScenes.Add(scene.name);
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            loadedScenes.Remove(scene.name);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }
}