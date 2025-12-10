using UnityEngine;
using System.Collections;

public class GameIntroManager : MonoBehaviour
{
    public float animDuration = 3.0f;
    [SerializeField] private GameObject introUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (introUI != null) introUI.SetActive(false);
    }
    public void ShowIntro()
    {
        if (introUI != null) introUI.SetActive(true);
    }
    // Update is called once per frame
    public void HideIntro()
    {
        if (introUI != null) introUI.SetActive(false);
    }
}