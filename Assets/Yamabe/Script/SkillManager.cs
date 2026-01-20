using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }
    [SerializeField] public List<SkillData> skillData=new List<SkillData>();
    private List<SkillData> savedSkillData = new List<SkillData>();
    private Dictionary<SkillData,float> cooldownTime=new Dictionary<SkillData,float>();

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Title") 
        {
            SaveState();
        }
        GameObject spawnPoint = GameObject.Find("SpawnPoint");

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }
    }
    public void SaveState()
    {
        savedSkillData = new List<SkillData>(skillData);
        Debug.Log($"[SkillManager] をバックアップした。今のスキル数: {savedSkillData.Count}");
    }

    public void RestoreState()
    {
        skillData = new List<SkillData>(savedSkillData);
        Debug.Log("[SkillManager] をロードした");
    }

    public void FullReset()
    {
        skillData.Clear();
        savedSkillData.Clear();
        cooldownTime.Clear();
    }
    //移動速度倍率
    public float TotalMoveSpeedMultiplier
    {
        get
        {
            float multiplier = 1f;
            foreach(var skill in skillData)
            {
                if (skill.Type == SkillType.StatAdjustment)
                    multiplier *= skill.MoveSpeedMultiplier;
            }
            return multiplier;
        }
    }

    //攻撃力倍率
    public float TotalDamageMultiplier
    {
        get
        {
            float multiplier = 1f;
            foreach (var skill in skillData)
            {
                if(skill.Type== SkillType.StatAdjustment)
                    multiplier *= skill.DamageMultiplier;
            }
            return multiplier;
        }
    }

    //攻撃速度倍率
    public float TotalAttackARateMultiplier
    {
        get
        {
            float multiplier = 1f;
            foreach( var skill in skillData)
            {
                if (skill.Type == SkillType.StatAdjustment)
                    multiplier *= skill.AttackRateMultiplier;
            }
            return multiplier;
        }
    }

    //攻撃回数
    public float TotalExtraAttack
    {
        get
        {
            int count = 0;
            foreach( var skill in skillData)
            {
                if (skill.Type == SkillType.StatAdjustment)
                    count += skill.ExtraAttacks;
            }
            return count;
        }
    }

    //ライフスティールの回復倍率
    public float TotalLifestealRatio
    {
        get
        {
            float ratio = 0f;
            foreach (var skill in skillData)
            {
                if (skill.LifestealRatio > 0)
                {
                    ratio += skill.LifestealRatio;
                }
            }
            return ratio;
        }
    }

    public bool HomingShot
    {
        get
        {
            bool flag = false;
            foreach (var skill in skillData)
            {
                if (skill.IsHomingShot) 
                {
                    flag = true;
                }
            }
            return flag;
        }
    }

    public bool ChargeAttack
    {
        get
        {
            bool flag = false;
            foreach (var skill in skillData)
            {
                if (skill.IsChargeAttack)
                {
                    flag = true;
                }
            }
            return flag;
        }
    }

    void Update()
    {
        HandleActiveSkills();
    }

    private void HandleActiveSkills()
    {
        foreach( var skill in skillData)
        {
            if(!cooldownTime.ContainsKey(skill))
            {
                cooldownTime[skill] = 0f;
            }

            if (Time.time < cooldownTime[skill])
            {
                continue;
            }

            bool alreadyActive = false;

            foreach (Transform child in transform)
            {

                if (child.name.StartsWith(skill.EffectPrefab.name))
                {
                    alreadyActive = true;
                    break;
                }
            }

            if (alreadyActive) continue;


            bool shouldActivtate = false;

            if(skill.Type==SkillType.AutomaticAttack)
            {
                shouldActivtate = true;
            }
            else if(skill.Type==SkillType.TriggeredAbility)
            {
                if(Input.GetKeyDown("q"))
                {
                    shouldActivtate = true;
                }
            }

            //発動処理
            if(shouldActivtate)
            {
                ActivateSkillEffect(skill);
                cooldownTime[skill] = Time.time + skill.CooldownDuration;
            }
        }
    }

    private void ActivateSkillEffect(SkillData skill)
    {
        if(skill.EffectPrefab!=null)
        {
            Instantiate(skill.EffectPrefab,transform.position,transform.rotation,transform);

        }
    }

    public void AddSkill(SkillData newSkill)
    {
        SkillData oldSkill = skillData.Find(s => s.NextLevelSkill == newSkill);
        if (oldSkill != null)
        {
            skillData.Remove(oldSkill);
            Debug.Log($" {oldSkill.SkillName}がレベルアップ");
        }
        skillData.Add(newSkill);
    }
}
