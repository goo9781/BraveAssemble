using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BAUnitView : MonoBehaviour
{
    private BAUnitViewModel _viewModel;

    public string Id => _viewModel?.Id;
    public string UnitType => _viewModel?.UnitType;
    public float MaxHealth => _viewModel?.MaxHealth ?? 0f;
    public float AttackDamage => _viewModel?.AttackDamage ?? 0f;
    public float MoveSpeed => _viewModel?.MoveSpeed ?? 0f;
    public float DetectionRange => _viewModel?.DetectionRange ?? 0f;
    public float AttackRange => _viewModel?.AttackRange ?? 0f;
    public float AttackInterval => _viewModel?.AttackInterval ?? 0f;
    public float CurrentHealth => _viewModel?.CurrentHealth ?? 0f;
    public bool IsDead => _viewModel == null || _viewModel.IsDead;

    public event Action Died;

    public void Bind(BAUnitViewModel viewModel)
    {
        if (viewModel == null)
        {
            throw new ArgumentNullException(nameof(viewModel));
        }

        Unbind();

        _viewModel = viewModel;
        _viewModel.Died += OnDied;
    }

    public void Unbind()
    {
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.Died -= OnDied;
        _viewModel = null;
    }

    public void TakeDamage(float damage)
    {
        _viewModel?.TakeDamage(damage);
    }

    public void RestoreHealth(float amount)
    {
        _viewModel?.RestoreHealth(amount);
    }

    public void ResetState()
    {
        if (_viewModel == null)
        {
            return;
        }

        gameObject.SetActive(true);
        _viewModel.ResetState();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void OnDied()
    {
        Died?.Invoke();
        gameObject.SetActive(false);
    }
}
