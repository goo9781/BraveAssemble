using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BASupportManager : MonoBehaviour
{
    [SerializeField] private string _supportId;
    [SerializeField] private Transform _entryPoint;
    [SerializeField] private Transform _actionPoint;
    [SerializeField] private Transform _exitPoint;

    private BADataManager _dataManager;
    private BAAssetManager _assetManager;
    private BAPoolManager _poolManager;
    private BABattleManager _battleManager;
    private BAStageManager _stageManager;
    private BAAssembleManager _assembleManager;
    private BASupportModel _supportModel;
    private bool _isInitialized;
    private bool _isInitializing;

    public static BASupportManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;
    public string DisplayName => _supportModel?.DisplayName;
    public string PrefabKey => _supportModel?.PrefabKey;
    public string EffectType => _supportModel?.EffectType;
    public float BaseEffectValue => _supportModel?.BaseEffectValue ?? 0f;
    public float Cooldown => _supportModel?.Cooldown ?? 0f;
    public float RemainingCooldown => _supportModel?.RemainingCooldown ?? 0f;
    public float MoveSpeed => _supportModel?.MoveSpeed ?? 0f;
    public int NormalMaxTargetCount => _supportModel?.NormalMaxTargetCount ?? 0;
    public float AssembledEffectMultiplier => _supportModel?.AssembledEffectMultiplier ?? 0f;
    public float AssembledRange => _supportModel?.AssembledRange ?? 0f;
    public int AssembledMaxTargetCount => _supportModel?.AssembledMaxTargetCount ?? 0;
    public float EffectDuration => _supportModel?.EffectDuration ?? 0f;
    public bool CanUse =>
        _isInitialized &&
        _supportModel != null &&
        _supportModel.CanUse &&
        !_stageManager.IsStageEnded;
    public Transform EntryPoint => _entryPoint;
    public Transform ActionPoint => _actionPoint;
    public Transform ExitPoint => _exitPoint;

    public event Action<float, float> CooldownChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator InitializeAsync(
        BADataManager dataManager,
        BAAssetManager assetManager,
        BAPoolManager poolManager,
        BABattleManager battleManager,
        BAStageManager stageManager,
        BAAssembleManager assembleManager)
    {
        if (_isInitialized)
        {
            yield break;
        }

        if (_isInitializing)
        {
            while (_isInitializing)
            {
                yield return null;
            }

            yield break;
        }

        _isInitializing = true;

        if (dataManager == null || !dataManager.IsInitialized)
        {
            Debug.LogError("BADataManager가 없거나 초기화되지 않아 서포트 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (assetManager == null || !assetManager.IsInitialized)
        {
            Debug.LogError("BAAssetManager가 없거나 초기화되지 않아 서포트 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (poolManager == null || !poolManager.IsInitialized)
        {
            Debug.LogError("BAPoolManager가 없거나 초기화되지 않아 서포트 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (battleManager == null || !battleManager.IsInitialized)
        {
            Debug.LogError("BABattleManager가 없거나 초기화되지 않아 서포트 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (stageManager == null || !stageManager.IsInitialized)
        {
            Debug.LogError("BAStageManager가 없거나 초기화되지 않아 서포트 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (assembleManager == null || !assembleManager.IsInitialized)
        {
            Debug.LogError("BAAssembleManager가 없거나 초기화되지 않아 서포트 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (string.IsNullOrWhiteSpace(_supportId))
        {
            Debug.LogError("서포트 ID가 설정되지 않아 서포트 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (_entryPoint == null || _actionPoint == null || _exitPoint == null)
        {
            Debug.LogError("서포트 진입, 행동, 퇴장 지점 Transform이 모두 설정되지 않았습니다.");
            _isInitializing = false;
            yield break;
        }

        if (!dataManager.TryGetSupportData(_supportId, out BASupportData supportData))
        {
            Debug.LogError($"서포트 데이터를 찾을 수 없습니다: {_supportId}");
            _isInitializing = false;
            yield break;
        }

        if (string.IsNullOrWhiteSpace(supportData.PrefabKey) ||
            string.IsNullOrWhiteSpace(supportData.EffectType))
        {
            Debug.LogError($"서포트 프리팹 키 또는 효과 유형이 유효하지 않습니다: {_supportId}");
            _isInitializing = false;
            yield break;
        }

        if (supportData.BaseEffectValue <= 0f ||
            supportData.Cooldown < 0f ||
            supportData.MoveSpeed <= 0f ||
            supportData.NormalMaxTargetCount <= 0 ||
            supportData.AssembledEffectMultiplier <= 0f ||
            supportData.AssembledRange <= 0f ||
            supportData.AssembledMaxTargetCount <= 0 ||
            supportData.EffectDuration < 0f)
        {
            Debug.LogError($"서포트 수치 데이터가 유효하지 않습니다: {_supportId}");
            _isInitializing = false;
            yield break;
        }

        _dataManager = dataManager;
        _assetManager = assetManager;
        _poolManager = poolManager;
        _battleManager = battleManager;
        _stageManager = stageManager;
        _assembleManager = assembleManager;
        _supportModel = new BASupportModel(supportData);
        _supportModel.CooldownChanged += OnCooldownChanged;

        GameObject supportPrefab = null;

        yield return _assetManager.LoadPrefabAsync(
            _supportModel.PrefabKey,
            prefab => supportPrefab = prefab);

        if (supportPrefab == null)
        {
            Debug.LogError($"서포트 프리팹을 불러오지 못했습니다: {_supportModel.PrefabKey}");
            ClearSupportModel();
            _isInitializing = false;
            yield break;
        }

        if (!_poolManager.RegisterPool(_supportModel.PrefabKey, supportPrefab, 1))
        {
            Debug.LogError($"서포트 오브젝트 풀 등록에 실패했습니다: {_supportModel.PrefabKey}");
            ClearSupportModel();
            _isInitializing = false;
            yield break;
        }

        _isInitialized = true;
        _isInitializing = false;
        CooldownChanged?.Invoke(_supportModel.RemainingCooldown, _supportModel.Cooldown);
    }

    private void Update()
    {
        if (!_isInitialized || _supportModel == null)
        {
            return;
        }

        _supportModel.UpdateCooldown(Time.deltaTime);
    }

    private void OnCooldownChanged(float remainingCooldown, float cooldown)
    {
        CooldownChanged?.Invoke(remainingCooldown, cooldown);
    }

    private void ClearSupportModel()
    {
        if (_supportModel == null)
        {
            return;
        }

        _supportModel.CooldownChanged -= OnCooldownChanged;
        _supportModel = null;
    }

    private void OnDestroy()
    {
        ClearSupportModel();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
