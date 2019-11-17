namespace d4160.Systems.SceneManagement
{
    using System;
    using UniRx.Async;
    using UnityEngine;

    public interface IUniRxAsyncSceneLoader
    {
        UniTask LoadSceneAsync(int buildIdx, bool setActive = false, Action<AsyncOperation> onStarted = null, Action onCompleted = null, bool allowSceneActivation = true, AsyncOperationProgress onProgress = null);

        UniTask UnloadSceneAsync(int buildIdx, Action onCompleted = null);
    }
}
