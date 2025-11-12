using UnityEngine;

public class CardFanLayout : MonoBehaviour
{
    [Tooltip("カード間の距離")]
    [SerializeField]private float cardSpacing = 150f;

    [Tooltip("傾き角度")]
    [SerializeField] private float fanAngle = 10f;

    [Tooltip("真ん中のカード以外の幅")]
    [SerializeField] private float riseFactor = 20f;

    public void ArrangeCards()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        //中心のインデクス
        float centerIndex = (childCount - 1) / 2f;

        for (int i = 0; i < childCount; i++)
        {
            Transform card = transform.GetChild(i);
            RectTransform cardRect = card.GetComponent<RectTransform>();

            float xPos = (i - centerIndex) * cardSpacing;
            float yPos = -Mathf.Abs(i - centerIndex) * riseFactor;
            cardRect.anchoredPosition = new Vector2(xPos, yPos);

            float angle = -(i - centerIndex) * fanAngle;
            cardRect.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}