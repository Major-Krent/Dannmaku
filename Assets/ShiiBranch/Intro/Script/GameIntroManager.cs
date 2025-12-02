using UnityEngine;
using System.Collections;

public class GameIntroManager : MonoBehaviour
{
    [SerializeField] private float animDuration = 3.0f;
    [SerializeField] private GameObject introUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    // Update is called once per frame
    IEnumerator PlayIntroSequence()
    {

        yield return new WaitForSeconds(animDuration);

    }
}