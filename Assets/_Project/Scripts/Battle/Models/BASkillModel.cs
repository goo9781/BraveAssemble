using System;

public class BASkillModel
{
    private readonly string _id;
    private readonly string _displayName;
    private readonly string _skillType;
    private readonly float _damageMultiplier;
    private readonly float _range;
    private readonly int _maxTargetCount;
    private readonly float _cooldown;

    private float _remainingCooldown;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string SkillType => _skillType;
    public float DamageMultiplier => _damageMultiplier;
    public float Range => _range;
    public int MaxTargetCount => _maxTargetCount;
    public float Cooldown => _cooldown;
    public float RemainingCooldown => _remainingCooldown;
    public bool CanUse => _remainingCooldown <= 0f;

    public event Action<float, float> CooldownChanged;

    public BASkillModel(BASkillData skillData)
    {
        if (skillData == null)
        {
            throw new ArgumentNullException(nameof(skillData));
        }

        _id = skillData.ID;
        _displayName = skillData.DisplayName;
        _skillType = skillData.SkillType;
        _damageMultiplier = Math.Max(0f, skillData.DamageMultiplier);
        _range = Math.Max(0f, skillData.Range);
        _maxTargetCount = Math.Max(0, skillData.MaxTargetCount);
        _cooldown = Math.Max(0f, skillData.Cooldown);
    }

    public bool TryStartCooldown()
    {
        if (!CanUse)
        {
            return false;
        }

        _remainingCooldown = _cooldown;
        CooldownChanged?.Invoke(_remainingCooldown, _cooldown);
        return true;
    }

    public void UpdateCooldown(float deltaTime)
    {
        if (deltaTime <= 0f || CanUse)
        {
            return;
        }

        _remainingCooldown = Math.Max(0f, _remainingCooldown - deltaTime);
        CooldownChanged?.Invoke(_remainingCooldown, _cooldown);
    }

    public void ResetState()
    {
        _remainingCooldown = 0f;
        CooldownChanged?.Invoke(_remainingCooldown, _cooldown);
    }
}
