using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class SkillCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SkillData currentSkill;
    public Image skillIcon;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;

    //デリゲートとイベント
    public delegate void SkillSelectedAction(SkillData skillData);
    public static event SkillSelectedAction OnSkillSelected;

    [Header("アニメーション")]
    [Tooltip("選択すると上に抽出距離")]
    public float hoverRiseAmount = 50f;
    [Tooltip("アニメーションのスピード")]
    public float animationSpeed = 10f;

    private Vector2 originalPosition;
    private Coroutine currentAnimation;
    void Awake()
    {
        //元の座標を記録
        originalPosition = GetComponent<RectTransform>().anchoredPosition;
    }
    public void UpdateOriginalPosition()
    {
        //元の座標を更新
        originalPosition = GetComponent<RectTransform>().anchoredPosition;
    }
    public void Setup(SkillData data)
    {
        currentSkill = data;
        skillIcon.sprite = data.Icon;
        skillNameText.text = data.SkillName;
        skillDescriptionText.text = data.Description;
    }

    //マウスで選択する仕組み
    public void OnPointerEnter(PointerEventData eventData)
    {
        // スキル内容を表示させる
        //TooltipManager.Instance.ShowTooltip(currentSkill.Description);
        AnimateCard(originalPosition + new Vector2(0, hoverRiseAmount));
        //カードを一番前にする
        transform.SetAsLastSibling();
        Debug.Log( currentSkill.Description+ "に入る");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // スキル内容を非表示させる
        //TooltipManager.Instance.HideTooltip();
        AnimateCard(originalPosition);
    }
    private void AnimateCard(Vector2 targetPosition)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(MoveCardCoroutine(targetPosition));
    }

    IEnumerator MoveCardCoroutine(Vector2 target)
    {
        RectTransform rect = GetComponent<RectTransform>();
        
        while (Vector2.Distance(rect.anchoredPosition, target) > 0.1f)
        {

            rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            target,
            Time.unscaledDeltaTime * animationSpeed);
            yield return null; 
        }
        
        rect.anchoredPosition = target;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        //TooltipManager.Instance.HideTooltip();
        // 選択されたスキル
        if (OnSkillSelected != null)
        {
            OnSkillSelected(currentSkill);
        }
        Debug.Log( currentSkill.SkillName+ "を選択された");
    }
}
