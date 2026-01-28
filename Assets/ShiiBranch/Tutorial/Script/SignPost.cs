using UnityEngine;

public class SignPost : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject uiPanel;

    private bool isPlayerInZone;
    [SerializeField]private GameObject hintIcon;
    [SerializeField]private KeyCode interactKey = KeyCode.E;
    void Start()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        if (hintIcon != null) hintIcon.SetActive(false);
    }
    void Update()
    {
        if (isPlayerInZone)
        {
            if (Input.GetKeyDown(interactKey))
            {
                ToggleInteraction();
            }
        }
    }
    void ToggleInteraction()
    {
        bool isUiOpen = uiPanel.activeSelf;

        if (isUiOpen)
        {
            CloseContent();
        }
        else
        {
            OpenContent();
        }
    }

    void OpenContent()
    {
        uiPanel.SetActive(true);
        if (hintIcon != null) hintIcon.SetActive(false);

        Time.timeScale = 0; 
    }

    void CloseContent()
    {
        uiPanel.SetActive(false);
        if (hintIcon != null) hintIcon.SetActive(true);

        Time.timeScale = 1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            if (hintIcon != null) hintIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            uiPanel.SetActive(false);
            if (hintIcon != null) hintIcon.SetActive(false);

            Time.timeScale = 1; 
        }
    }
}
