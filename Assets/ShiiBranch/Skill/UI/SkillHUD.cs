using UnityEngine;

public class SkillHUD : MonoBehaviour
{
    public GameObject blankSkillIcon;

    public SkillData blankSkillData; 

    void Update()
    {
        if (SkillManager.Instance == null) return;

        bool hasBlank = SkillManager.Instance.skillData.Contains(blankSkillData);

        if (hasBlank && !blankSkillIcon.activeSelf)
        {
            blankSkillIcon.SetActive(true);
        }
    }
}
