using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BAUnitView : MonoBehaviour
{
    private BAUnitViewModel _viewModel;

    public string Id => _viewModel?.Id;
    public string UnitType => _viewModel?.UnitType;
    public float CurrentHealth => _viewModel?.CurrentHealth ?? 0f;
    public bool IsDead => _viewModel == null || _viewModel.IsDead;

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
        gameObject.SetActive(false);
    }
}
