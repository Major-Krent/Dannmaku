using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SelectionManager : MonoBehaviour
{
    [SerializeField]private CanvasGroup fadeCanvasGroup;
    [SerializeField]private float fadeDuration = 1.0f;

    [SerializeField] private string gameSceneName = "GameScene";

    private bool isTransitioning = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnCharacterSelected(CharacterCard selectedCard)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        selectedCard.LockCard();

        StartCoroutine(SelectionSequence(selectedCard));
    }
    IEnumerator SelectionSequence(CharacterCard card)
    {
        //アニメーション
        if (card.charAnimator != null)
        {
           // card.charAnimator.SetTrigger("Attack"); 

           //yield return null;

           //float animLength = card.charAnimator.GetCurrentAnimatorStateInfo(0).length;

           //yield return new WaitForSeconds(animLength);
        }
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

        //選択されたenumを保存
        GameContext.SelectedCharacter = card.characterType;
        SceneManager.LoadScene(gameSceneName);
    }
}
