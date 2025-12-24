using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    public Image cooldownImage;

    public KeyCode skillKey = KeyCode.Q;
    public float cooldownTime = 5f;

    private float timer = 0f;
    private bool isCooldown = false;

    void Update()
    {
        if (Input.GetKeyDown(skillKey) && !isCooldown)
        {
            isCooldown = true;
            timer = cooldownTime;
        }

        if (isCooldown)
        {
            timer -= Time.deltaTime;
            cooldownImage.fillAmount = timer / cooldownTime;

            if (timer <= 0)
            {
                isCooldown = false;
                cooldownImage.fillAmount = 0;
            }
        }
    }
    public void StartCooldown()
    {
        if (!isCooldown)
        {
            isCooldown = true;
            timer = cooldownTime;
        }
    }
}
