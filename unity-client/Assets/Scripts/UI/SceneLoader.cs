using System;
using System.Collections;
using NitroRush.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroRush.UI
{
    /// <summary>
    /// Async Scene Loading service wrapper with progress callbacks.
    /// </summary>
    public static class SceneLoader
    {
        public static void LoadScene(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        {
            CoroutineRunner.Instance.StartCoroutine(LoadSceneRoutine(sceneName, onProgress, onComplete));
        }

        private static IEnumerator LoadSceneRoutine(string sceneName, Action<float> onProgress, Action onComplete)
        {
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = false;

            while (!asyncOp.isDone)
            {
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                onProgress?.Invoke(progress);

                if (asyncOp.progress >= 0.9f)
                {
                    asyncOp.allowSceneActivation = true;
                }

                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}
