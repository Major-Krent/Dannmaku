using UnityEngine;
using System.Collections;

public class BossBattleManager : MonoBehaviour
{
    public static BossBattleManager Instance { get; private set; }

    [Header("BOSS")]
    [SerializeField] private GameObject bossGameObject;
    [Header("UI")]
    [SerializeField] private GameObject bossHealthUI;

    [SerializeField] private GameObject EntryDoor;
    [Header("Intro")]
    [SerializeField] private GameIntroManager introManager;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioClip oceanBGM;
    [SerializeField] private AudioClip summonSE; 
    [SerializeField] private AudioClip battleBGM; 

    private bool hasStarted = false;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (bossGameObject != null)
            bossGameObject.SetActive(false);
        if (bossHealthUI != null)
            bossHealthUI.SetActive(false);
        if (EntryDoor != null)
            EntryDoor.SetActive(false);

        PlayBGM(oceanBGM, 1.0f);
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
        StartCoroutine(FadeOutBGM(1.0f));
        if (seSource != null && summonSE != null)
        {
            seSource.PlayOneShot(summonSE);
        }
        if (introManager != null)
        {
            introManager.ShowIntro();
            yield return new WaitForSeconds(introManager.animDuration);
            introManager.HideIntro();
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }
        if (bossGameObject != null)
        {
            bossGameObject.SetActive(true);
        }
        if (bossHealthUI != null)
        {
            bossHealthUI.SetActive(true);
        }
        PlayBGM(battleBGM, 0.5f);
    }
    public void OnBossDefeated()
    {

        StartCoroutine(FadeOutBGM(2.0f));

        StartCoroutine(ResumeOceanBGM(2.5f));

        if (bossHealthUI != null) bossHealthUI.SetActive(false);
        if (EntryDoor != null) EntryDoor.SetActive(false); 
    }
    IEnumerator ResumeOceanBGM(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayBGM(oceanBGM, 2.0f);
    }
    private void PlayBGM(AudioClip clip, float fadeDuration = 0f)
    {
        if (bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();

        if (fadeDuration > 0)
        {
            bgmSource.volume = 0;
            StartCoroutine(FadeInBGM(fadeDuration));
        }
        else
        {
            bgmSource.volume = 1;
        }
    }
    IEnumerator FadeOutBGM(float duration)
    {
        if (bgmSource == null) yield break;
        float startVol = bgmSource.volume;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0, t / duration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVol;
    }

    IEnumerator FadeInBGM(float duration)
    {
        if (bgmSource == null) yield break;
        float targetVol = 1.0f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0, targetVol, t / duration);
            yield return null;
        }
        bgmSource.volume = targetVol;
    }
}
