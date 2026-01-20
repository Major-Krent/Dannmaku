using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [SerializeField]private GameObject gameOverPanel;
    [Header("アニメーション")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float delayBeforeGameOver = 3.0f;
    private CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroupがない");
        }

        gameOverPanel.SetActive(false);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        Time.timeScale = 1f;
    }
    public void ShowGameOver()
    {
        StartCoroutine(GameOverSequence());
    }
    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);
        gameOverPanel.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        StartCoroutine(FadeInCoroutine());

    }
    private IEnumerator FadeInCoroutine()
    {

        Time.timeScale = 0f;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            }
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        
    }
    //リスタートボタン
    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        //プレイヤーのスキルを初期化
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.RestoreState();
            Player_Controller player = FindFirstObjectByType<Player_Controller>();
            if (player != null) player.Revive();
        }
        //スキルプルを初期化
        if (SkillSelectionManager.Instance != null)
        {
            SkillSelectionManager.Instance.RestorePoolState();
        }

        //ステージをリロードする
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    //タイトルボタン
    public void OnTitleClicked()
    {
        Time.timeScale = 1f;
        
        if (SkillManager.Instance != null)
        {
            Destroy(SkillManager.Instance.gameObject);
        }

        if (SkillSelectionManager.Instance != null)
        {
            Destroy(SkillSelectionManager.Instance.transform.root.gameObject);
        }

        SceneManager.LoadScene("Title"); 
    }
}
