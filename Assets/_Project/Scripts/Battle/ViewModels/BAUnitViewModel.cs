using System;

public class BAUnitViewModel : IDisposable
{
    private readonly BAUnitModel _model;

    private bool _isDisposed;

    public string Id => _model.Id;
    public string DisplayName => _model.DisplayName;
    public string UnitType => _model.UnitType;
    public float MaxHealth => _model.MaxHealth;
    public float AttackDamage => _model.AttackDamage;
    public float MoveSpeed => _model.MoveSpeed;
    public float DetectionRange => _model.DetectionRange;
    public float AttackRange => _model.AttackRange;
    public float AttackInterval => _model.AttackInterval;
    public float CurrentHealth => _model.CurrentHealth;
    public bool IsDead => _model.IsDead;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    public BAUnitViewModel(BAUnitModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _model.HealthChanged += OnHealthChanged;
        _model.Died += OnDied;
    }

    public void TakeDamage(float damage)
    {
        _model.TakeDamage(damage);
    }

    public void RestoreHealth(float amount)
    {
        _model.RestoreHealth(amount);
    }

    public void ResetState()
    {
        _model.ResetState();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _model.HealthChanged -= OnHealthChanged;
        _model.Died -= OnDied;
        _isDisposed = true;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnDied()
    {
        Died?.Invoke();
    }
}
