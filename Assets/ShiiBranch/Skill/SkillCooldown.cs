using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    public Image cooldownImage;
    public float cooldownTime = 5f;
    private float timer = 0f;
    private bool isCooldown = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isCooldown)
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
}
