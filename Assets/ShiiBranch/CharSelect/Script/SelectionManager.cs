using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private int nextSceneIndex = 2;
    private bool isTransitioning = false;
    public SceneMana sceneMana;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnCharacterSelected(CharacterCard selectedCard)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        selectedCard.LockCard();
        //選択されたenumを保存
        GameContext.SelectedCharacter = selectedCard.characterType;
        Debug.Log($"{selectedCard.characterType}");

        if (SkillManager.Instance != null)
        {
            Destroy(SkillManager.Instance.gameObject);
        }

        SceneMana sceneMana = FindFirstObjectByType<SceneMana>();

        if (sceneMana != null)
        {
            sceneMana.LoadLevelByIndex(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
    
}
