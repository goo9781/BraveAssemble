using System;

public class BASupportModel
{
    private readonly string _id;
    private readonly string _displayName;
    private readonly string _prefabKey;
    private readonly string _effectType;
    private readonly float _baseEffectValue;
    private readonly float _cooldown;
    private readonly float _moveSpeed;
    private readonly int _normalMaxTargetCount;
    private readonly float _assembledEffectMultiplier;
    private readonly float _assembledRange;
    private readonly int _assembledMaxTargetCount;
    private readonly float _effectDuration;

    private float _remainingCooldown;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string PrefabKey => _prefabKey;
    public string EffectType => _effectType;
    public float BaseEffectValue => _baseEffectValue;
    public float Cooldown => _cooldown;
    public float MoveSpeed => _moveSpeed;
    public int NormalMaxTargetCount => _normalMaxTargetCount;
    public float AssembledEffectMultiplier => _assembledEffectMultiplier;
    public float AssembledRange => _assembledRange;
    public int AssembledMaxTargetCount => _assembledMaxTargetCount;
    public float EffectDuration => _effectDuration;
    public float RemainingCooldown => _remainingCooldown;
    public bool CanUse => _remainingCooldown <= 0f;

    public event Action<float, float> CooldownChanged;

    public BASupportModel(BASupportData supportData)
    {
        if (supportData == null)
        {
            throw new ArgumentNullException(nameof(supportData));
        }

        _id = supportData.ID;
        _displayName = supportData.DisplayName;
        _prefabKey = supportData.PrefabKey;
        _effectType = supportData.EffectType;
        _baseEffectValue = Math.Max(0f, supportData.BaseEffectValue);
        _cooldown = Math.Max(0f, supportData.Cooldown);
        _moveSpeed = Math.Max(0f, supportData.MoveSpeed);
        _normalMaxTargetCount = Math.Max(0, supportData.NormalMaxTargetCount);
        _assembledEffectMultiplier = Math.Max(0f, supportData.AssembledEffectMultiplier);
        _assembledRange = Math.Max(0f, supportData.AssembledRange);
        _assembledMaxTargetCount = Math.Max(0, supportData.AssembledMaxTargetCount);
        _effectDuration = Math.Max(0f, supportData.EffectDuration);
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

        float nextRemainingCooldown = Math.Max(0f, _remainingCooldown - deltaTime);

        if (_remainingCooldown == nextRemainingCooldown)
        {
            return;
        }

        _remainingCooldown = nextRemainingCooldown;
        CooldownChanged?.Invoke(_remainingCooldown, _cooldown);
    }

    public void ResetState()
    {
        _remainingCooldown = 0f;
        CooldownChanged?.Invoke(_remainingCooldown, _cooldown);
    }
}
