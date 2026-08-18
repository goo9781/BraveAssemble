using System;

public class BABattleHudViewModel : IDisposable
{
    private const string _heroUnitType = "Hero";

    private readonly BABattleManager _battleManager;
    private readonly BAStageManager _stageManager;
    private readonly BASkillManager _skillManager;
    private readonly BAAssembleManager _assembleManager;

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
    public string AssembleDisplayName => _assembleManager.DisplayName;
    public float AssembleMaxGauge => _assembleManager.MaxGauge;
    public float AssembleCurrentGauge => _assembleManager.CurrentGauge;
    public float AssembleDuration => _assembleManager.Duration;
    public float AssembleRemainingDuration => _assembleManager.RemainingDuration;
    public bool IsAssembled => _assembleManager.IsAssembled;
    public bool CanAssemble => _assembleManager.CanAssemble;
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
    public event Action<float, float> AssembleGaugeChanged;
    public event Action<float, float> AssembleDurationChanged;
    public event Action<bool> AssembleStateChanged;

    public BABattleHudViewModel(
        BABattleManager battleManager,
        BAStageManager stageManager,
        BASkillManager skillManager,
        BAAssembleManager assembleManager)
    {
        _battleManager = battleManager ?? throw new ArgumentNullException(nameof(battleManager));
        _stageManager = stageManager ?? throw new ArgumentNullException(nameof(stageManager));
        _skillManager = skillManager ?? throw new ArgumentNullException(nameof(skillManager));
        _assembleManager = assembleManager ?? throw new ArgumentNullException(nameof(assembleManager));

        if (!_skillManager.IsInitialized)
        {
            throw new ArgumentException("스킬 매니저가 초기화되지 않았습니다.", nameof(skillManager));
        }

        if (!_assembleManager.IsInitialized)
        {
            throw new ArgumentException("합체 매니저가 초기화되지 않았습니다.", nameof(assembleManager));
        }

        _battleManager.UnitBound += OnUnitBound;
        _stageManager.RemainingEnemyCountChanged += OnRemainingEnemyCountChanged;
        _stageManager.StageCleared += OnStageCleared;
        _stageManager.StageFailed += OnStageFailed;
        _skillManager.CooldownChanged += OnSkillCooldownChanged;
        _assembleManager.GaugeChanged += OnAssembleGaugeChanged;
        _assembleManager.DurationChanged += OnAssembleDurationChanged;
        _assembleManager.AssembleStateChanged += OnAssembleStateChanged;

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

    public void RequestAssemble()
    {
        if (_isDisposed)
        {
            return;
        }

        _assembleManager.TryStartAssemble();
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
        _assembleManager.GaugeChanged -= OnAssembleGaugeChanged;
        _assembleManager.DurationChanged -= OnAssembleDurationChanged;
        _assembleManager.AssembleStateChanged -= OnAssembleStateChanged;

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

    private void OnAssembleGaugeChanged(float currentGauge, float maxGauge)
    {
        AssembleGaugeChanged?.Invoke(currentGauge, maxGauge);
    }

    private void OnAssembleDurationChanged(float remainingDuration, float duration)
    {
        AssembleDurationChanged?.Invoke(remainingDuration, duration);
    }

    private void OnAssembleStateChanged(bool isAssembled)
    {
        AssembleStateChanged?.Invoke(isAssembled);
    }
}
