using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoad : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;
            SceneMana manager = FindFirstObjectByType<SceneMana>();
            if (manager != null)
            {
                manager.LoadLevelByIndex(nextSceneIndex);
            }

            else
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
        }
           
    }

}
