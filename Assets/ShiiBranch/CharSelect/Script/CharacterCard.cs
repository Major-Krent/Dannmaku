using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]private float hoverScale = 1.2f;
    [SerializeField] private float smoothTime = 0.1f;
    public GameContext.CharacterType characterType;  //0:‹ß1:‰“

    public Animator charAnimator;
    [SerializeField] private SelectionManager manager;  

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private bool isLocked = false; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked) return;
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * hoverScale));

        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked) return;
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLocked) return;

        manager.OnCharacterSelected(this);
    }
    public void LockCard()
    {
        isLocked = true;
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float timer = 0;

        while (timer < smoothTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, target, timer / smoothTime);
            yield return null;
        }
        transform.localScale = target;
    }
}
