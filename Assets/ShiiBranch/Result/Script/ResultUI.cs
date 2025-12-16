using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ResultUI : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI level1Text;
    [SerializeField] private TextMeshProUGUI level2Text;
    [SerializeField] private TextMeshProUGUI level3Text;
    [SerializeField] private TextMeshProUGUI totalText;
    [SerializeField] private GameObject backButtonObject;
    [Header("Timer")]
    [SerializeField] private float startDelay = 4f;
    [SerializeField] private float interval = 2f; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        level1Text.gameObject.SetActive(false);
        level2Text.gameObject.SetActive(false);
        level3Text.gameObject.SetActive(false);
        totalText.gameObject.SetActive(false);
        backButtonObject.SetActive(false);
        StartCoroutine(ShowResultSequence());
    }

    // Update is called once per frame
    IEnumerator ShowResultSequence()
    {
       
        float time1 = GameContext.LevelTimes[0];
        float time2 = GameContext.LevelTimes[1];
        float time3 = GameContext.LevelTimes[2]; 
        float totalTime = time1 + time2 + time3;
  
        level1Text.text = $"Level 1: <color=#5A2222>{FormatTime(time1)}</color>";
        level2Text.text = $"Level 2: <color=#5A2222>{FormatTime(time2)}</color>";
        level3Text.text = $"Level 3: <color=#5A2222>{FormatTime(time3)}</color>";
        totalText.text = $"TOTAL:   <color=#5A2222>{FormatTime(totalTime)}</color>";

        yield return new WaitForSeconds(startDelay);


        level1Text.gameObject.SetActive(true);
        //AudioSource.PlayClipAtPoint(popSound, transform.position);
        yield return new WaitForSeconds(interval);

        level2Text.gameObject.SetActive(true);
        yield return new WaitForSeconds(interval);

        level3Text.gameObject.SetActive(true);
        yield return new WaitForSeconds(interval);

        yield return new WaitForSeconds(interval + 0.5f);
        totalText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        backButtonObject.SetActive(true);
    }
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100) % 100);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
    public void BackToTitle()
    {
        GameContext.ResetData(); 
        
        SceneMana sceneMana = FindFirstObjectByType<SceneMana>();
        if (sceneMana != null)
        {
            sceneMana.LoadLevelByIndex(0); 
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
