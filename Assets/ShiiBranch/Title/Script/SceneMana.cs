using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneMana : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 2.0f;


    private bool isTransitioning = false;

    void Start()
    {
        fadeCanvasGroup.alpha = 1;
        fadeCanvasGroup.blocksRaycasts = true;
        StartCoroutine(FadeIn());
    }
    public void LoadLevelByIndex(int sceneIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeOutAndLoad(sceneIndex));
    }

    IEnumerator FadeOutAndLoad(int sceneIndex)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;


        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;

        if (sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"ƒV[ƒ“‚ª‚ ‚è‚Ü‚¹‚ñ");
        }
    }
    IEnumerator FadeIn()
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.alpha = 1;

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.blocksRaycasts = false;

        isTransitioning = false;
    }
}
