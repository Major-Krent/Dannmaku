using UnityEngine;
using System.Collections.Generic;

//スキルレベル
public enum SkillTier
{
    Normal,   
    Advanced //操作できるスキル
}
//スキル種類
public enum SkillType
{
    StatAdjustment,     //数値調整
    AutomaticAttack,    //自動追加スキル（バリアなど）
    TriggeredAbility,   //キー追加スキル（追跡弾など）
    AttackModifier      //攻撃パターン修正（2回攻撃するとか）
}
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("基本")]
    [SerializeField] private string _skillName;         //スキル名
    [SerializeField, TextArea(3, 10)] private string _description;       //スキル内容
    [SerializeField] private Sprite _icon;              //スキルアイコン

    [Header("スキル実装内容")]
    [SerializeField] private SkillType _type;           //スキル種類
    [SerializeField, Tooltip("スキルのレベル")]
    private SkillTier _tier;

    [Header("数値調整(StatAdjustment)")]
    [SerializeField, Tooltip("乗算値。1.2にすると120%、0.8にすると80%。")]
    private float _moveSpeedMultiplier = 1f;
    [SerializeField, Tooltip("乗算値。1.2にすると120%、0.8にすると80%。")]
    private float _damageMultiplier = 1f;
    [SerializeField, Tooltip("乗算値。1.2にすると120%、0.8にすると80%。")]
    private float _attackRateMultiplier = 1f;

    [Header("攻撃パターン修正 (AttackModifier)")]
    [SerializeField,Tooltip("一回の攻撃で追加する攻撃回数。")]
    public int _extraAttacks; //攻撃回数
    [SerializeField, Tooltip("与えたダメージの内、HPとして回復する割合。0.1にすると10%回復。")]
    private float _lifestealRatio;

    [Header("自動攻撃・新しいスキル")]
    [SerializeField] private GameObject _effectPrefab; //バリア、追跡弾のPrefab

    [SerializeField, Tooltip("クールダウン時間（秒),0の場合はクールダウンなし。")]
    private float _cooldownDuration;//-= Time.deltaTime

    public string SkillName => _skillName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public SkillType Type => _type;
    public float MoveSpeedMultiplier => _moveSpeedMultiplier;
    public float DamageMultiplier => _damageMultiplier;
    public float AttackRateMultiplier => _attackRateMultiplier;
    public int ExtraAttacks => _extraAttacks;
    public GameObject EffectPrefab => _effectPrefab;
    public float LifestealRatio => _lifestealRatio;
    public SkillTier Tier => _tier;
    public float CooldownDuration => _cooldownDuration;
}