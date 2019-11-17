namespace d4160.Systems.SceneManagement
{
    using UnityEngine.AddressableAssets;
    using System;
    using System.Collections;
    using d4160.Core;
    using d4160.Loops;
    using UnityEngine.SceneManagement;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using System.Threading.Tasks;
    using UnityEngine;

    public class AddressablesSceneManagementSingleton : Singleton<AddressablesSceneManagementSingleton>
    {
        #region Fields
        protected AsyncOperationHandle<SceneInstance> m_asyncLoadOperation;
        protected bool m_asyncLoadOperationRegistered;
        #endregion

        #region Action and Events
        /// <summary>
        /// To store scene load callbacks to start after the current ends
        /// </summary>
        protected event Action m_onAsyncLoadOperationHelper;
        protected event AsyncOperationProgress m_onAsyncLoadOperationProgress;
        #endregion

        #region Unity Callbacks
        protected virtual void OnEnable()
        {
            UpdateLoop.OnUpdate += OnUpdate;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            UpdateLoop.OnUpdate -= OnUpdate;
        }
        #endregion

        #region Loop Methods
        protected void OnUpdate(float deltaTime)
        {
            if (m_asyncLoadOperationRegistered)
                m_onAsyncLoadOperationProgress?.Invoke(m_asyncLoadOperation.PercentComplete);
        }
        #endregion

        #region Load Methods
        public static Task<SceneInstance> LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool activateOnLoad = true)
        {
            if (!SceneManagementSingleton.IsSceneLoadedOrInBackground(sceneName))
            {
                var ao = Addressables.LoadSceneAsync(sceneName, mode, activateOnLoad);

                return ao.Task;
            }
            else
            {
                return Task.FromResult<SceneInstance>(default);
            }
        }

        public static void LoadScene(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            Action<AsyncOperationHandle<SceneInstance>> onComplete = null)
        {
            if (!SceneManagementSingleton.IsSceneLoadedOrInBackground(sceneName))
            {
                Instance.StartCoroutine(LoadSceneCo(sceneName, mode, activateOnLoad, onComplete));
            }
            else
            {
                onComplete?.Invoke(default);
            }
        }

        protected static IEnumerator LoadSceneCo(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            Action<AsyncOperationHandle<SceneInstance>> onComplete = null)
        {
            var ao = Addressables.LoadSceneAsync(sceneName, mode, activateOnLoad);
            if (onComplete != null)
                    ao.Completed += onComplete;

            yield return ao;
        }

        public virtual Task<SceneInstance> LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            AsyncOperationProgress onProgress = null)
        {
            if (!SceneManagementSingleton.IsSceneLoadedOrInBackground(sceneName))
            {
                if (!m_asyncLoadOperationRegistered)
                {
                    m_onAsyncLoadOperationHelper = null;

                    return LoadSceneAsyncInternal(sceneName, mode, activateOnLoad, null, onProgress);
                }
            }

            return Task.FromResult<SceneInstance>(default);
        }

        public virtual void LoadScene(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            Action<AsyncOperationHandle<SceneInstance>> onCompleted = null,
            AsyncOperationProgress onProgress = null)
        {
            if (!SceneManagementSingleton.IsSceneLoadedOrInBackground(sceneName))
            {
                if (!m_asyncLoadOperationRegistered)
                {
                    m_onAsyncLoadOperationHelper = null;

                    LoadSceneAsyncInternal(sceneName, mode, activateOnLoad, onCompleted, onProgress);
                }
                else
                {
                    m_onAsyncLoadOperationHelper += () =>
                    {
                        LoadScene(sceneName, mode, activateOnLoad, onCompleted, onProgress);
                    };
                }
            }
            else
            {
                onCompleted?.Invoke(default);
            }
        }

        protected virtual Task<SceneInstance> LoadSceneAsyncInternal(
            string sceneName,
            LoadSceneMode mode,
            bool activateOnLoad,
            Action<AsyncOperationHandle<SceneInstance>> onCompleted = null,
            AsyncOperationProgress onProgress = null)
        {
            StartCoroutine(LoadSceneInternalCo(sceneName, mode, activateOnLoad));

            m_asyncLoadOperationRegistered = true;

            if (onCompleted != null)
                m_asyncLoadOperation.Completed += onCompleted;

            if (onProgress != null)
                m_onAsyncLoadOperationProgress += onProgress;

            m_asyncLoadOperation.Completed += OnAsyncLoadOperationCompletedCallback;

            return m_asyncLoadOperation.Task;
        }

        protected virtual IEnumerator LoadSceneInternalCo(
            string sceneName,
            LoadSceneMode mode,
            bool activateOnLoad)
        {
            yield return m_asyncLoadOperation = Addressables.LoadSceneAsync(sceneName, mode, activateOnLoad);
        }

        protected virtual void OnAsyncLoadOperationCompletedCallback(AsyncOperationHandle<SceneInstance> ao)
        {
            m_asyncLoadOperationRegistered = false;

            m_onAsyncLoadOperationProgress = null;

            m_onAsyncLoadOperationHelper?.Invoke();
        }
        #endregion

        #region Unload Methods
        public static void UnloadScene(
            AsyncOperationHandle<SceneInstance> handle, 
            bool autoReleaseHandle = true,
            Action<AsyncOperationHandle<SceneInstance>> onCompleted = null)
        {
            if (handle.Result.Scene.isLoaded)
            {
                //StartCoroutine(UnloadSceneCo(sceneBuildIdx, onComplete));
                var resultHandle = Addressables.UnloadSceneAsync(handle, autoReleaseHandle);

                if (onCompleted != null)
                    resultHandle.Completed += onCompleted;
            }
        }

        public static Task<SceneInstance> UnloadSceneAsync(
            AsyncOperationHandle<SceneInstance> handle,
            bool autoReleaseHandle = true)
        {
            if (handle.Result.Scene.isLoaded)
            {
                //StartCoroutine(UnloadSceneCo(sceneBuildIdx, onComplete));
                return Addressables.UnloadSceneAsync(handle, autoReleaseHandle).Task;
            }

            return Task.FromResult<SceneInstance>(default);
        }

        public static void UnloadScene(
            SceneInstance handle,
            bool autoReleaseHandle = true,
            Action<AsyncOperationHandle<SceneInstance>> onCompleted = null)
        {
            if (handle.Scene.isLoaded)
            {
                //StartCoroutine(UnloadSceneCo(sceneBuildIdx, onComplete));
                var resultHandle = Addressables.UnloadSceneAsync(handle, autoReleaseHandle);

                if (onCompleted != null)
                    resultHandle.Completed += onCompleted;
            }
        }

        public static Task<SceneInstance> UnloadSceneAsync(
            SceneInstance handle,
            bool autoReleaseHandle = true)
        {
            if (handle.Scene.isLoaded)
            {
                //StartCoroutine(UnloadSceneCo(sceneBuildIdx, onComplete));
                return Addressables.UnloadSceneAsync(handle, autoReleaseHandle).Task;
            }

            return Task.FromResult<SceneInstance>(default);
        }
        #endregion
    }

    public static class AssetReferenceExtensions
    {
        public static Task<SceneInstance> LoadSceneAsync(
            this AssetReference instance,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool activateOnLoad = true)
        {
            Debug.Log($"AssetName: {instance.Asset.name}");
            //if (!SceneManagementSingleton.GetScene(instance.Asset.name).isLoaded)
            //{
                var ao = Addressables.LoadSceneAsync(instance, mode, activateOnLoad);

                return ao.Task;
            /*}
            else
            {
                return Task.FromResult<SceneInstance>(default);
            }*/
        }
    }
}
