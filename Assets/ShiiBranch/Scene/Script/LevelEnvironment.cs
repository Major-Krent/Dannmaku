using UnityEngine;

public class LevelEnvironment : MonoBehaviour
{
    [SerializeField] private GameObject obstacleMap;//è·äQï®Çè¡Ç∑
    [SerializeField] private GameObject hiddenPathMap;//âBÇÍÇΩìπÇï\é¶

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (obstacleMap != null) obstacleMap.SetActive(true);
        if (hiddenPathMap != null) hiddenPathMap.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OpenPath()
    {
        Debug.Log("êVÇµÇ¢ìπÇ™èoÇÈ");


        if (obstacleMap != null)
        {
            obstacleMap.SetActive(false);
            // Effect:Instantiate(smokeEffect, ...);
        }
        if (hiddenPathMap != null)
        {
            hiddenPathMap.SetActive(true);
        }

    }
}
