using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private int levelIndex = 0;

    private float currentTime = 0f;
    private bool isRunning = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
        }
    }
    public void StopAndRecord()
    {
        if (!isRunning) return;

        isRunning = false;

        if (levelIndex < GameContext.LevelTimes.Length)
        {
            GameContext.LevelTimes[levelIndex] = currentTime;
            Debug.Log($" ステージ{levelIndex + 1} の記録時間： {currentTime} 秒");
        }
    }
}
