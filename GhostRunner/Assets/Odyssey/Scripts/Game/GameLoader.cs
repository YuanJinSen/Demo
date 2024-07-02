using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Odyssey
{
    public class GameLoader : Singleton<GameLoader>
    {
        public UnityEvent onLoadStart;
        public UnityEvent onLoadFinish;
        public UIAinmator loadingScreen;
        [Header("MinTime")]
        public float startDelay = 1f;
        public float finishDelay = 1f;

        public bool isLoading { get; protected set; }
        public float loadingProgress { get; protected set; }
        public string currentScene => SceneManager.GetActiveScene().name;

        #region Private

        protected IEnumerator LoadRoutine(string scene)
        {
            isLoading = true;
            onLoadStart?.Invoke();
            loadingScreen.SetActive(true);
            loadingScreen.Show();

            yield return new WaitForSeconds(startDelay);

            var operation = SceneManager.LoadSceneAsync(scene);
            loadingProgress = 0.0f;
            while (!operation.isDone)
            {
                loadingProgress = operation.progress;
                yield return null;
            }
            loadingProgress = 1.0f;

            yield return new WaitForSeconds(finishDelay);

            loadingScreen.Hide();
            onLoadFinish?.Invoke();
            isLoading = false;
        }

        #endregion

        #region Public

        public void Load(string scene)
        {
            if (isLoading || currentScene == scene) return;
            StartCoroutine(LoadRoutine(scene));
        }

        public void Reload()
        {
            StartCoroutine(LoadRoutine(currentScene));
        }

        #endregion
    }
}