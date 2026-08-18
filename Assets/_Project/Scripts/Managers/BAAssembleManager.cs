using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BAAssembleManager : MonoBehaviour
{
    private const string _heroUnitType = "Hero";

    [SerializeField] private string _assembleId;

    private BABattleManager _battleManager;
    private BAStageManager _stageManager;
    private BAAssembleModel _assembleModel;
    private BAUnitView _assembledHero;
    private bool _isInitialized;

    public static BAAssembleManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;
    public string DisplayName => _assembleModel?.DisplayName;
    public string SupportPrefabKey => _assembleModel?.SupportPrefabKey;
    public float MaxGauge => _assembleModel?.MaxGauge ?? 0f;
    public float CurrentGauge => _assembleModel?.CurrentGauge ?? 0f;
    public float Duration => _assembleModel?.Duration ?? 0f;
    public float RemainingDuration => _assembleModel?.RemainingDuration ?? 0f;
    public float AttackDamageMultiplier => _assembleModel?.AttackDamageMultiplier ?? 0f;
    public float MoveSpeedMultiplier => _assembleModel?.MoveSpeedMultiplier ?? 0f;
    public float AttackSpeedMultiplier => _assembleModel?.AttackSpeedMultiplier ?? 0f;
    public bool IsAssembled => _assembleModel != null && _assembleModel.IsAssembled;
    public bool CanAssemble =>
        _isInitialized &&
        _assembleModel != null &&
        _assembleModel.CanAssemble &&
        !_stageManager.IsStageEnded;

    public event Action<float, float> GaugeChanged;
    public event Action<float, float> DurationChanged;
    public event Action<bool> AssembleStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool Initialize(
        BADataManager dataManager,
        BABattleManager battleManager,
        BAStageManager stageManager)
    {
        if (_isInitialized)
        {
            return true;
        }

        if (dataManager == null || !dataManager.IsInitialized)
        {
            Debug.LogError("BADataManager가 없거나 초기화되지 않아 합체 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (battleManager == null || !battleManager.IsInitialized)
        {
            Debug.LogError("BABattleManager가 없거나 초기화되지 않아 합체 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (stageManager == null || !stageManager.IsInitialized)
        {
            Debug.LogError("BAStageManager가 없거나 초기화되지 않아 합체 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_assembleId))
        {
            Debug.LogError("합체 ID가 설정되지 않아 합체 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (!dataManager.TryGetAssembleData(_assembleId, out BAAssembleData assembleData))
        {
            Debug.LogError($"합체 데이터를 찾을 수 없습니다: {_assembleId}");
            return false;
        }

        if (string.IsNullOrWhiteSpace(assembleData.SupportPrefabKey))
        {
            Debug.LogError($"합체 서포트 프리팹 키가 비어 있습니다: {_assembleId}");
            return false;
        }

        if (assembleData.MaxGauge <= 0f ||
            assembleData.GaugeGainPerHit <= 0f ||
            assembleData.Duration <= 0f)
        {
            Debug.LogError($"합체 게이지 또는 지속시간 데이터가 유효하지 않습니다: {_assembleId}");
            return false;
        }

        if (assembleData.AttackDamageMultiplier <= 0f ||
            assembleData.MoveSpeedMultiplier <= 0f ||
            assembleData.AttackSpeedMultiplier <= 0f)
        {
            Debug.LogError($"합체 능력치 배율 데이터가 유효하지 않습니다: {_assembleId}");
            return false;
        }

        _battleManager = battleManager;
        _stageManager = stageManager;
        _assembleModel = new BAAssembleModel(assembleData);
        _assembleModel.GaugeChanged += OnGaugeChanged;
        _assembleModel.DurationChanged += OnDurationChanged;
        _assembleModel.AssembleStateChanged += OnAssembleStateChanged;
        _battleManager.DamageApplied += OnDamageApplied;
        _isInitialized = true;

        GaugeChanged?.Invoke(_assembleModel.CurrentGauge, _assembleModel.MaxGauge);
        DurationChanged?.Invoke(_assembleModel.RemainingDuration, _assembleModel.Duration);
        AssembleStateChanged?.Invoke(_assembleModel.IsAssembled);
        return true;
    }

    private void Update()
    {
        if (!_isInitialized || _assembleModel == null)
        {
            return;
        }

        _assembleModel.UpdateDuration(Time.deltaTime);
    }

    public bool TryStartAssemble()
    {
        if (!CanAssemble)
        {
            return false;
        }

        if (!_battleManager.TryGetFirstActiveUnitByType(
                _heroUnitType,
                out BAUnitView heroView))
        {
            return false;
        }

        if (!heroView.ApplyCombatModifiers(
                _assembleModel.AttackDamageMultiplier,
                _assembleModel.MoveSpeedMultiplier,
                _assembleModel.AttackSpeedMultiplier))
        {
            return false;
        }

        _assembledHero = heroView;

        if (!_assembleModel.TryStartAssemble())
        {
            heroView.ResetCombatModifiers();
            _assembledHero = null;
            return false;
        }

        return true;
    }

    private void OnDamageApplied(
        BAUnitView attacker,
        BAUnitView target,
        float damage)
    {
        if (attacker == null)
        {
            return;
        }

        if (!_isInitialized || _stageManager.IsStageEnded)
        {
            return;
        }

        if (attacker.UnitType != _heroUnitType)
        {
            return;
        }

        _assembleModel.AddGaugeByHit();
    }

    private void OnGaugeChanged(float currentGauge, float maxGauge)
    {
        GaugeChanged?.Invoke(currentGauge, maxGauge);
    }

    private void OnDurationChanged(float remainingDuration, float duration)
    {
        DurationChanged?.Invoke(remainingDuration, duration);
    }

    private void OnAssembleStateChanged(bool isAssembled)
    {
        if (!isAssembled && _assembledHero != null)
        {
            _assembledHero.ResetCombatModifiers();
            _assembledHero = null;
        }

        AssembleStateChanged?.Invoke(isAssembled);
    }

    private void OnDestroy()
    {
        if (_assembledHero != null)
        {
            _assembledHero.ResetCombatModifiers();
            _assembledHero = null;
        }

        if (_battleManager != null)
        {
            _battleManager.DamageApplied -= OnDamageApplied;
        }

        if (_assembleModel != null)
        {
            _assembleModel.GaugeChanged -= OnGaugeChanged;
            _assembleModel.DurationChanged -= OnDurationChanged;
            _assembleModel.AssembleStateChanged -= OnAssembleStateChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
