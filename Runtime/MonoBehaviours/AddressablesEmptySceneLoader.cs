namespace d4160.Systems.SceneManagement
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UniRx.Async;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;

    public class AddressablesEmptySceneLoader : MonoBehaviour
    {
        private List<SceneInstance> _loadedScenes = new List<SceneInstance>();

        #if UNITY_EDITOR
        private List<string> _loadedScenesInEditor = new List<string>();
        #endif

        public async UniTask<SceneInstance> LoadSceneAsync(
            string sceneName, 
            bool activateOnLoad = true, 
            bool setActiveAsMainScene = false, 
            AsyncOperationProgress onProgress = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return default;
            }

            var scene = await AddressablesSceneManagementSingleton.Instance.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Additive,
                activateOnLoad, onProgress);

            // Already loaded?
            if (!scene.Scene.IsValid())
            {
                #if UNITY_EDITOR
                if (!_loadedScenesInEditor.Contains(sceneName))
                    _loadedScenesInEditor.Add(sceneName);
                /* TODO: Check if the sceneName is already in _loadedScenes, when is trying to load a scene with activateOnLoad false */
                #endif

                if (setActiveAsMainScene)
                    SceneManagementSingleton.SetActiveScene(sceneName);

                return default;
            }

            if (!_loadedScenes.Contains(scene))
                _loadedScenes.Add(scene);
            
            if (setActiveAsMainScene && activateOnLoad)
            {
                SceneManagementSingleton.SetActiveScene(scene.Scene);
            }

            return scene;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="setActiveAsMainScene"></param>
        /// <param name="onProgress"></param>
        /// <returns></returns>
        public async UniTask<SceneInstance> LoadSceneAsync(
            AssetReference asset, 
            bool activateOnLoad = true, 
            bool setActiveAsMainScene = false, 
            AsyncOperationProgress onProgress = null)
        {
            /*var scene = await AddressablesSceneManagementSingleton.Instance.LoadSceneAsync(
                asset,
                LoadSceneMode.Additive,
                activateOnLoad, onProgress);

            if (!scene.Scene.IsValid())
            {
                #if UNITY_EDITOR
                if (!_loadedScenesInEditor.Contains(sceneName))
                    _loadedScenesInEditor.Add(sceneName);
                #endif

                return default;
            }

            if (!_loadedScenes.Contains(scene))
                _loadedScenes.Add(scene);
            
            if (setActive && activateOnLoad)
            {
                SceneManagementSingleton.SetActiveScene(scene.Scene);
            }

            return scene;*/
            await UniTask.Yield();

            return default;
        }

        public async UniTask UnloadAllLoadedScenes(Action onCompleted = null)
        {
            for (int i = _loadedScenes.Count - 1; i >= 0; i--)
            {
                var result = await AddressablesSceneManagementSingleton.UnloadSceneAsync(_loadedScenes[i]);
                
                if(result.Scene.IsValid())
                    _loadedScenes.RemoveAt(i);
            }

            #if UNITY_EDITOR
            for (int i = _loadedScenesInEditor.Count - 1; i >= 0; i--)
            {
                SceneManagementSingleton.UnloadSceneAsync(_loadedScenesInEditor[i]);
                
                _loadedScenesInEditor.RemoveAt(i);
            }
            #endif

            onCompleted?.Invoke();
        }
    } 
}