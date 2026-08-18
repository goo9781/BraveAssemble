using System;

public class BAUnitModel
{
    private readonly string _id;
    private readonly string _displayName;
    private readonly string _unitType;
    
    private readonly float _maxHealth;
    private readonly float _attackDamage;
    private readonly float _moveSpeed;
    private readonly float _detectionRange;
    private readonly float _attackRange;
    private readonly float _attackInterval;

    private float _currentHealth;
    private bool _isDead;
    private float _attackDamageMultiplier = 1f;
    private float _moveSpeedMultiplier = 1f;
    private float _attackSpeedMultiplier = 1f;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string UnitType => _unitType;
    public float MaxHealth => _maxHealth;
    public float AttackDamage => _attackDamage * _attackDamageMultiplier;
    public float MoveSpeed => _moveSpeed * _moveSpeedMultiplier;
    public float DetectionRange => _detectionRange;
    public float AttackRange => _attackRange;
    public float AttackInterval =>
        _attackSpeedMultiplier > 0f
            ? _attackInterval / _attackSpeedMultiplier
            : _attackInterval;
    public float CurrentHealth => _currentHealth;
    public bool IsDead => _isDead;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    public BAUnitModel(BAUnitData unitData)
    {
        if (unitData == null)
        {
            throw new ArgumentNullException(nameof(unitData));
        }
        
        _id = unitData.ID;
        _displayName = unitData.DisplayName;
        _unitType = unitData.UnitType;
        _maxHealth = unitData.MaxHealth;
        _attackDamage = unitData.AttackDamage;
        _moveSpeed = unitData.MoveSpeed;
        _detectionRange = unitData.DetectionRange;
        _attackRange = unitData.AttackRange;
        _attackInterval = unitData.AttackInterval;
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || _isDead)
        {
            return;
        }

        _currentHealth = Math.Max(0f, _currentHealth - damage);

        if (_currentHealth <= 0f)
        {
            _isDead = true;
        }

        HealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_isDead)
        {
            Died?.Invoke();
        }
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f || _isDead)
        {
            return;
        }

        _currentHealth = Math.Min(_currentHealth + amount, _maxHealth);
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public bool ApplyCombatModifiers(
        float attackDamageMultiplier,
        float moveSpeedMultiplier,
        float attackSpeedMultiplier)
    {
        if (attackDamageMultiplier <= 0f ||
            moveSpeedMultiplier <= 0f ||
            attackSpeedMultiplier <= 0f)
        {
            return false;
        }

        _attackDamageMultiplier = attackDamageMultiplier;
        _moveSpeedMultiplier = moveSpeedMultiplier;
        _attackSpeedMultiplier = attackSpeedMultiplier;
        return true;
    }

    public void ResetCombatModifiers()
    {
        _attackDamageMultiplier = 1f;
        _moveSpeedMultiplier = 1f;
        _attackSpeedMultiplier = 1f;
    }

    public void ResetState()
    {
        _currentHealth = _maxHealth;
        _isDead = false;
        ResetCombatModifiers();
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
}
