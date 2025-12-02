using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneMana : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1.0f;

    [SerializeField] private string gameSceneName = "GameScene";

    private bool isTransitioning = false;

    public void StartGame()
    {
        
        StartCoroutine(SelectionSequence());
    }
    IEnumerator SelectionSequence()
    {
        //FadeOut
        fadeCanvasGroup.blocksRaycasts = true;
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;
        SceneManager.LoadScene(gameSceneName);
    }
}
