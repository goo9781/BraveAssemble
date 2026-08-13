using System;

public class BABattleHudViewModel : IDisposable
{
    private const string _heroUnitType = "Hero";

    private readonly BABattleManager _battleManager;
    private readonly BAStageManager _stageManager;

    private BAUnitViewModel _heroViewModel;
    private bool _isDisposed;

    public float HeroMaxHealth => _heroViewModel?.MaxHealth ?? 0f;
    public float HeroCurrentHealth => _heroViewModel?.CurrentHealth ?? 0f;
    public int RemainingEnemyCount => _stageManager.RemainingEnemyCount;
    public bool IsStageCleared => _stageManager.IsStageCleared;
    public bool IsStageFailed => _stageManager.IsStageFailed;

    public event Action<float, float> HeroHealthChanged;
    public event Action<int> RemainingEnemyCountChanged;
    public event Action StageCleared;
    public event Action StageFailed;

    public BABattleHudViewModel(
        BABattleManager battleManager,
        BAStageManager stageManager)
    {
        _battleManager = battleManager ?? throw new ArgumentNullException(nameof(battleManager));
        _stageManager = stageManager ?? throw new ArgumentNullException(nameof(stageManager));

        _battleManager.UnitBound += OnUnitBound;
        _stageManager.RemainingEnemyCountChanged += OnRemainingEnemyCountChanged;
        _stageManager.StageCleared += OnStageCleared;
        _stageManager.StageFailed += OnStageFailed;

        if (_battleManager.TryGetFirstUnitViewModelByType(
                _heroUnitType,
                out BAUnitViewModel heroViewModel))
        {
            BindHeroViewModel(heroViewModel);
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _battleManager.UnitBound -= OnUnitBound;
        _stageManager.RemainingEnemyCountChanged -= OnRemainingEnemyCountChanged;
        _stageManager.StageCleared -= OnStageCleared;
        _stageManager.StageFailed -= OnStageFailed;

        if (_heroViewModel != null)
        {
            _heroViewModel.HealthChanged -= OnHeroHealthChanged;
            _heroViewModel = null;
        }

        _isDisposed = true;
    }

    private void OnUnitBound(BAUnitViewModel unitViewModel)
    {
        if (unitViewModel == null || unitViewModel.UnitType != _heroUnitType)
        {
            return;
        }

        BindHeroViewModel(unitViewModel);
    }

    private void BindHeroViewModel(BAUnitViewModel heroViewModel)
    {
        if (_heroViewModel != null)
        {
            _heroViewModel.HealthChanged -= OnHeroHealthChanged;
        }

        _heroViewModel = heroViewModel;
        _heroViewModel.HealthChanged += OnHeroHealthChanged;
        HeroHealthChanged?.Invoke(_heroViewModel.CurrentHealth, _heroViewModel.MaxHealth);
    }

    private void OnHeroHealthChanged(float currentHealth, float maxHealth)
    {
        HeroHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnRemainingEnemyCountChanged(int remainingEnemyCount)
    {
        RemainingEnemyCountChanged?.Invoke(remainingEnemyCount);
    }

    private void OnStageCleared()
    {
        StageCleared?.Invoke();
    }

    private void OnStageFailed()
    {
        StageFailed?.Invoke();
    }
}
