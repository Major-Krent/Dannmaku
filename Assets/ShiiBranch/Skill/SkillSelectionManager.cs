using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class SkillSelectionManager : MonoBehaviour
{
    public static SkillSelectionManager Instance { get; private set; }

    [SerializeField] private SkillCard skillCardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private RectTransform skillSelectionPanel;
    [Header("スキル")]
    //[SerializeField] private List<SkillData> allSkills;
    [SerializeField, Tooltip("ゲームに登場する全てのスキル（マスターリスト）")]
    private List<SkillData> masterSkillList;

    private List<SkillData> availableSkillPool;


    [Header("アニメーション")]
    [SerializeField] private float animationSpeed = 15f;
    [SerializeField] private float offScreenYPosition = -1200f;

    private CardFanLayout cardLayout;
    private Vector2 onScreenPosition = Vector2.zero;
    private Vector2 offScreenPosition;

    private bool isInteractable = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        cardLayout = cardContainer.GetComponent<CardFanLayout>();

        offScreenPosition = new Vector2(0, offScreenYPosition);

        skillSelectionPanel.anchoredPosition = offScreenPosition;
        skillSelectionPanel.gameObject.SetActive(false);

        InitializeSkillPool();
    }
    /// <summary>
    /// スキルプールを初期化・リセットする
    /// </summary>
    public void InitializeSkillPool()
    {
        availableSkillPool = new List<SkillData>(masterSkillList);
        Debug.Log($"スキルプールを初期化しました。利用可能なスキル数: {availableSkillPool.Count}");
    }
    //-------------------------テスト用--------------------------
    //private void Update()
    //{
 
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        if (cardContainer.childCount == 0)
    //        {
    //            TriggerSkillSelection(3);
    //        }
    //    }
    //}
    //----------------------------------------------------------
    private void OnEnable()
    {

        SkillCard.OnSkillSelected += OnSkillChosen;
    }

    private void OnDisable()
    {
        SkillCard.OnSkillSelected -= OnSkillChosen;
    }

    public void TriggerSkillSelection(int amountToPick, bool forceOneAdvanced = false)
    {
        StartCoroutine(ShowPanelAndCards(amountToPick, forceOneAdvanced));
    }
    /// <summary>
    /// カードUIを呼び出す
    /// </summary>
    /// <param name="amountToPick">抽出するカードの数</param>
    private IEnumerator ShowPanelAndCards(int amountToPick, bool forceOneAdvanced)
    {
        isInteractable = false;
        Time.timeScale = 0f;

        skillSelectionPanel.gameObject.SetActive(true);
       

        List<SkillData> finalPicks = new List<SkillData>();

        if (forceOneAdvanced)
        {
            //ボス戦

            //普通スキルと高級スキルを分ける
            List<SkillData> normalPool = availableSkillPool.Where(skill => skill.Tier == SkillTier.Normal).ToList();
            List<SkillData> advancedPool = availableSkillPool.Where(skill => skill.Tier == SkillTier.Advanced).ToList();

            if (advancedPool.Count == 0)
            {
                Debug.LogError("高級スキルがない！普通スキルに代わる");
                // Fallback to normal selection
                SelectFromPool(normalPool, amountToPick, ref finalPicks);
            }
            else
            {
                //高級スキルから一つ選ぶ
                Shuffle(advancedPool);
                finalPicks.Add(advancedPool[0]);

                //普通スキルから残りを選ぶ
                int remainingAmount = amountToPick - 1;
                Shuffle(normalPool);
                for (int i = 0; i < remainingAmount && i < normalPool.Count; i++)
                {
                    finalPicks.Add(normalPool[i]);
                }
            }
            
        }
        else
        {
            //普通スキル
            List<SkillData> normalPool = availableSkillPool.Where(skill => skill.Tier == SkillTier.Normal).ToList();
            SelectFromPool(normalPool, amountToPick, ref finalPicks);
        }

        Shuffle(finalPicks);

        foreach (var skillData in finalPicks)
        {
            SkillCard newCard = Instantiate(skillCardPrefab, cardContainer);
            newCard.Setup(skillData);
        }
        //レイアウト
        ArrangeAndFinalizeLayout();
        yield return StartCoroutine(AnimatePanelCoroutine(onScreenPosition));
        isInteractable = true;
    }
    private void OnSkillChosen(SkillData chosenSkill)
    {
        if (!isInteractable) return;
        isInteractable = false;
        if (availableSkillPool.Contains(chosenSkill))
        {
            availableSkillPool.Remove(chosenSkill);
            Debug.Log($"スキル「{chosenSkill.SkillName}」を選択。プールから削除しました。残りスキル数: {availableSkillPool.Count}");
        }
        StartCoroutine(HideAndCleanupPanel());
    }

    private IEnumerator HideAndCleanupPanel()
    {
        yield return StartCoroutine(AnimatePanelCoroutine(offScreenPosition));

        CleanupCards();
        skillSelectionPanel.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }
    private IEnumerator AnimatePanelCoroutine(Vector2 targetPosition)
    {
        while (Vector2.Distance(skillSelectionPanel.anchoredPosition, targetPosition) > 1f)
        {
            skillSelectionPanel.anchoredPosition = Vector2.Lerp(
            skillSelectionPanel.anchoredPosition,
            targetPosition,
            Time.unscaledDeltaTime * animationSpeed);
            yield return null;
        }
        skillSelectionPanel.anchoredPosition = targetPosition;
    }
    private void CleanupCards()
    {
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        //cardContainer.gameObject.SetActive(false);
    }

    private void SelectFromPool(List<SkillData> pool, int amount, ref List<SkillData> picks)
    {
        Shuffle(pool);
        for (int i = 0; i < amount && i < pool.Count; i++)
        {
            picks.Add(pool[i]);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void ArrangeAndFinalizeLayout()
    {
        if (cardLayout != null) cardLayout.ArrangeCards();
        foreach (Transform cardTransform in cardContainer)
        {
            cardTransform.GetComponent<SkillCard>()?.UpdateOriginalPosition();
        }
    }
}