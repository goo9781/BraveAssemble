using System;

public class BABattleHudViewModel : IDisposable
{
    private const string _heroUnitType = "Hero";

    private readonly BABattleManager _battleManager;
    private readonly BAStageManager _stageManager;
    private readonly BASkillManager _skillManager;

    private BAUnitViewModel _heroViewModel;
    private bool _isDisposed;
    private bool _isPaused;

    public float HeroMaxHealth => _heroViewModel?.MaxHealth ?? 0f;
    public float HeroCurrentHealth => _heroViewModel?.CurrentHealth ?? 0f;
    public int RemainingEnemyCount => _stageManager.RemainingEnemyCount;
    public bool IsStageCleared => _stageManager.IsStageCleared;
    public bool IsStageFailed => _stageManager.IsStageFailed;
    public string SkillDisplayName => _skillManager.DisplayName;
    public float SkillCooldown => _skillManager.Cooldown;
    public float SkillRemainingCooldown => _skillManager.RemainingCooldown;
    public bool CanUseSkill => _skillManager.CanUse;
    public bool IsPaused => _isPaused;

    public event Action<float, float> HeroHealthChanged;
    public event Action<int> RemainingEnemyCountChanged;
    public event Action StageCleared;
    public event Action StageFailed;
    public event Action RestartRequested;
    public event Action QuitRequested;
    public event Action PauseRequested;
    public event Action ResumeRequested;
    public event Action<bool> PauseStateChanged;
    public event Action<float, float> SkillCooldownChanged;

    public BABattleHudViewModel(
        BABattleManager battleManager,
        BAStageManager stageManager,
        BASkillManager skillManager)
    {
        _battleManager = battleManager ?? throw new ArgumentNullException(nameof(battleManager));
        _stageManager = stageManager ?? throw new ArgumentNullException(nameof(stageManager));
        _skillManager = skillManager ?? throw new ArgumentNullException(nameof(skillManager));

        if (!_skillManager.IsInitialized)
        {
            throw new ArgumentException("스킬 매니저가 초기화되지 않았습니다.", nameof(skillManager));
        }

        _battleManager.UnitBound += OnUnitBound;
        _stageManager.RemainingEnemyCountChanged += OnRemainingEnemyCountChanged;
        _stageManager.StageCleared += OnStageCleared;
        _stageManager.StageFailed += OnStageFailed;
        _skillManager.CooldownChanged += OnSkillCooldownChanged;

        if (_battleManager.TryGetFirstUnitViewModelByType(
                _heroUnitType,
                out BAUnitViewModel heroViewModel))
        {
            BindHeroViewModel(heroViewModel);
        }
    }

    public void RequestRestart()
    {
        if (_isDisposed)
        {
            return;
        }

        RestartRequested?.Invoke();
    }

    public void RequestQuit()
    {
        if (_isDisposed)
        {
            return;
        }

        QuitRequested?.Invoke();
    }

    public void RequestUseSkill()
    {
        if (_isDisposed)
        {
            return;
        }

        _skillManager.TryUseSkill();
    }

    public void RequestPause()
    {
        if (_isDisposed)
        {
            return;
        }

        PauseRequested?.Invoke();
    }

    public void RequestResume()
    {
        if (_isDisposed)
        {
            return;
        }

        ResumeRequested?.Invoke();
    }

    public void UpdateGameState(BAGameState gameState)
    {
        bool isPaused = gameState == BAGameState.Paused;

        if (_isPaused == isPaused)
        {
            return;
        }

        _isPaused = isPaused;
        PauseStateChanged?.Invoke(_isPaused);
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
        _skillManager.CooldownChanged -= OnSkillCooldownChanged;

        if (_heroViewModel != null)
        {
            _heroViewModel.HealthChanged -= OnHeroHealthChanged;
            _heroViewModel = null;
        }

        _isDisposed = true;
        RestartRequested = null;
        QuitRequested = null;
        PauseRequested = null;
        ResumeRequested = null;
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

    private void OnSkillCooldownChanged(float remainingCooldown, float cooldown)
    {
        SkillCooldownChanged?.Invoke(remainingCooldown, cooldown);
    }
}
