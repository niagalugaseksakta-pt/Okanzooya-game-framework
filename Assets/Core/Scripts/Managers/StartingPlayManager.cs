using System.Collections;
using UnityEngine;

public class StartingPlayManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainSceneName = "SampleScene"; // change to your real main scene

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Image progressBar;

    private float _target;
    void Start()
    {
        _target = 0;
        progressBar.fillAmount = 0;
        // Start async loading
        StartCoroutine(LoadMainSceneAsync());
    }

    private void Update()
    {
        float progress = Mathf.MoveTowards(progressBar.fillAmount, _target, 3 * Time.deltaTime);
        if (progressBar != null)
            progressBar.fillAmount = progress;
    }

    IEnumerator LoadMainSceneAsync()
    {
        // Begin async loading
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(mainSceneName);

        // Prevent immediate switch (wait until 100%)
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            // Progress is between 0.0f and 0.9f

            _target = asyncLoad.progress;
            // When loading is finished (progress = 0.9)
            if (asyncLoad.progress >= 0.9f)
            {
                // Optionally wait for a keypress or small delay
                yield return new WaitForSeconds(0.5f);

                // Allow the scene to activate
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
