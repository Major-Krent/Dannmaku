using UnityEngine;
using System.Collections;

public class BossBattleManager : MonoBehaviour
{
    [Header("BOSS")]
    [SerializeField] private GameObject bossGameObject;
    [Header("UI")]
    [SerializeField] private GameObject bossHealthUI;

    [SerializeField] private GameObject EntryDoor;
    [Header("Intro")]
    [SerializeField] private GameIntroManager introManager;
    private bool hasStarted = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (bossGameObject != null)
            bossGameObject.SetActive(false);
        if (bossHealthUI != null)
            bossHealthUI.SetActive(false);
        if (EntryDoor != null)
            EntryDoor.SetActive(false);
    }

    // Update is called once per frame
    public void StartBossBattle()
    {
        if (hasStarted) return;
        hasStarted = true;

        StartCoroutine(BattleSequence());
    }
    IEnumerator BattleSequence()
    {
        if (EntryDoor != null) 
            EntryDoor.SetActive(true);
        if (introManager != null)
        {
            introManager.ShowIntro();
            yield return new WaitForSeconds(introManager.animDuration);
            introManager.HideIntro();
        }
        if (bossGameObject != null)
        {
            bossGameObject.SetActive(true);
        }
        if (bossHealthUI != null)
        {
            bossHealthUI.SetActive(true);
        }
    }
}
