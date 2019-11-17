namespace d4160.Systems.SceneManagement
{
    using System;
    using UniRx.Async;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class UniRxAsyncSceneManagementSingleton
    {
        #region Load Methods
        public static UniTask LoadSceneAsync(
            int sceneBuildIdx,
            LoadSceneMode mode = LoadSceneMode.Single,
            Action<AsyncOperation> onStarted = null,
            Action onCompleted = null, 
            bool allowSceneActivation = true,
            AsyncOperationProgress onProgress = null)
        {
            if (sceneBuildIdx != -1 && !SceneManagementSingleton.IsSceneLoadedOrInBackground(sceneBuildIdx))
            {
                AsyncOperation ao = SceneManager.LoadSceneAsync(sceneBuildIdx, mode);
                ao.allowSceneActivation = allowSceneActivation;
                
                onStarted?.Invoke(ao);

                if (onCompleted != null)
                    ao.completed += (ctx) => onCompleted.Invoke();

                return ao.ConfigureAwait(Progress.Create<float>(x => onProgress?.Invoke(x)));
            }
            else
            {
                onCompleted?.Invoke();

                return UniTask.CompletedTask;
            }
        }

        public static UniTask LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            Action<AsyncOperation> onStarted = null,
            Action onCompleted = null,
            bool allowSceneActivation = true,
            AsyncOperationProgress onProgress = null)
        {
            if (!string.IsNullOrEmpty(sceneName) && !SceneManagementSingleton.IsSceneLoadedOrInBackground(sceneName))
            {
                AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, mode);
                ao.allowSceneActivation = allowSceneActivation;

                onStarted?.Invoke(ao);

                if (onCompleted != null)
                    ao.completed += (ctx) => onCompleted.Invoke();

                return ao.ConfigureAwait(Progress.Create<float>(x => onProgress?.Invoke(x)));
            }
            else
            {
                onCompleted?.Invoke();

                return UniTask.CompletedTask;
            }
        }

        public static UniTask LoadSceneAsync(
            int buildIndex,
            bool setActiveAsMainScene = false,
            Action<AsyncOperation> onStarted = null,
            Action onCompleted = null,
            bool allowSceneActivation = true, 
            AsyncOperationProgress onProgress = null,
            LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (setActiveAsMainScene)
            {
                return LoadSceneAsync(buildIndex, mode, onStarted, () =>
                {
                    if (setActiveAsMainScene)
                        SceneManagementSingleton.SetActiveScene(buildIndex);

                    onCompleted?.Invoke();
                }, 
                allowSceneActivation, onProgress);
            }
            else
            {
                return LoadSceneAsync(buildIndex, mode, onStarted,
                    () => onCompleted?.Invoke(), 
                    allowSceneActivation, onProgress);
            }
        }

        public static UniTask LoadSceneAsync(
            string sceneName,
            bool setActiveAsMainScene = false,
            Action<AsyncOperation> onStarted = null,
            Action onCompleted = null,
            bool allowSceneActivation = true,
            AsyncOperationProgress onProgress = null,
            LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (setActiveAsMainScene)
            {
                return LoadSceneAsync(sceneName, mode, onStarted, () =>
                {
                    if (setActiveAsMainScene)
                        SceneManagementSingleton.SetActiveScene(sceneName);

                    onCompleted?.Invoke();
                }, 
                allowSceneActivation, onProgress);
            }
            else
            {
                return LoadSceneAsync(sceneName, mode, onStarted,
                    () => onCompleted?.Invoke(),
                    allowSceneActivation, onProgress);
            }
        }
        #endregion

        #region Unload Methods
        public static UniTask UnloadSceneAsync(
            int sceneBuildIdx, 
            Action<AsyncOperation> onCompleted = null,
            UnloadSceneOptions options = UnloadSceneOptions.None,
            AsyncOperationProgress onProgress = null)
        {
            if (sceneBuildIdx != -1 && SceneManagementSingleton.IsSceneLoadedOrInBackground(sceneBuildIdx))
            {
                AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneBuildIdx, options);

                if (onCompleted != null)
                    ao.completed += onCompleted;

                return ao.ConfigureAwait(Progress.Create<float>(x => onProgress?.Invoke(x)));
            }
            else
            {
                onCompleted?.Invoke(null);

                return UniTask.CompletedTask;
            }
        }

        public static UniTask UnloadSceneAsync(
            string sceneName, 
            Action<AsyncOperation> onCompleted = null,
            UnloadSceneOptions options = UnloadSceneOptions.None,
            AsyncOperationProgress onProgress = null)
        {
            if (!string.IsNullOrEmpty(sceneName) && SceneManagementSingleton.IsSceneLoadedOrInBackground(sceneName))
            {
                AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneName, options);

                if (onCompleted != null)
                    ao.completed += onCompleted;

                return ao.ConfigureAwait(Progress.Create<float>(x => onProgress?.Invoke(x)));
            }
            else
            {
                onCompleted?.Invoke(null);

                return UniTask.CompletedTask;
            }
        }

        public static UniTask UnloadSceneAsync(
            int sceneBuildIdx,
            Action onCompleted = null,
            UnloadSceneOptions options = UnloadSceneOptions.None,
            AsyncOperationProgress onProgress = null)
        {
            return UnloadSceneAsync(
                sceneBuildIdx, (ao) => onCompleted?.Invoke(),
                options, onProgress);
        }

        public static UniTask UnloadSceneAsync(
            string sceneName,
            Action onCompleted = null,
            UnloadSceneOptions options = UnloadSceneOptions.None,
            AsyncOperationProgress onProgress = null)
        {
            return UnloadSceneAsync(
                sceneName, (ao) => onCompleted?.Invoke(),
                options, onProgress);
        }
        #endregion
    }
}