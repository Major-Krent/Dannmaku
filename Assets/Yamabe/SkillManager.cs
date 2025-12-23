using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }
    [SerializeField] public List<SkillData> skillData=new List<SkillData>();

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

        GameObject spawnPoint = GameObject.Find("SpawnPoint");

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }
    }
    //ˆÚ“®‘¬“x”{—¦
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

    //UŒ‚—Í”{—¦
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

    //UŒ‚‘¬“x”{—¦
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

    //UŒ‚‰ñ”
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

    //ƒ‰ƒCƒtƒXƒeƒB[ƒ‹‚Ì‰ñ•œ”{—¦
    public float TotalLifestealRatio
    {
        get
        {
            float ratio = 0f;
            foreach (var skill in skillData)
            {
                if (skill.Type == SkillType.StatAdjustment)
                    ratio += skill.LifestealRatio; 
            }
            return ratio;
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

            bool shouldActivtate = false;

            if(skill.Type==SkillType.AutomaticAttack)
            {
                shouldActivtate = true;
            }
            else if(skill.Type==SkillType.TriggeredAbility)
            {
                if(Input.GetKeyDown("Q"))
                {
                    shouldActivtate = true;
                }
            }

            //”­“®ˆ—
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
            Instantiate(skill.EffectPrefab,transform.position,transform.rotation);

        }
    }

    public void AddSkill(SkillData newSkill)
    {
        skillData.Add(newSkill);
    }
}
