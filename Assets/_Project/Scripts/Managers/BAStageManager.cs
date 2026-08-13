using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BAStageManager : MonoBehaviour
{
    private const string _heroUnitType = "Hero";

    [SerializeField] private List<BAEnemySpawner> _enemySpawners = new List<BAEnemySpawner>();

    private BABattleManager _battleManager;
    private BAUnitViewModel _heroViewModel;
    private bool _isInitialized;
    private bool _isBattleResultBound;
    private bool _isStageCleared;
    private bool _isStageFailed;

    public static BAStageManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;
    public bool IsStageCleared => _isStageCleared;
    public bool IsStageFailed => _isStageFailed;
    public bool IsStageEnded => _isStageCleared || _isStageFailed;
    public int RemainingEnemyCount
    {
        get
        {
            if (_enemySpawners == null)
            {
                return 0;
            }

            int remainingEnemyCount = 0;

            foreach (BAEnemySpawner enemySpawner in _enemySpawners)
            {
                if (enemySpawner != null)
                {
                    remainingEnemyCount += enemySpawner.RemainingEnemyCount;
                }
            }

            return Mathf.Max(0, remainingEnemyCount);
        }
    }

    public event Action StageCleared;
    public event Action StageFailed;
    public event Action<int> RemainingEnemyCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool Initialize()
    {
        if (_isInitialized)
        {
            return true;
        }

        if (_enemySpawners == null || _enemySpawners.Count == 0)
        {
            Debug.LogError("등록된 적 스포너가 없어 스테이지 매니저를 초기화할 수 없습니다.");
            return false;
        }

        HashSet<BAEnemySpawner> uniqueSpawners = new HashSet<BAEnemySpawner>();

        foreach (BAEnemySpawner enemySpawner in _enemySpawners)
        {
            if (enemySpawner == null)
            {
                Debug.LogError("적 스포너 목록에 null 항목이 있어 스테이지 매니저를 초기화할 수 없습니다.");
                return false;
            }

            if (!uniqueSpawners.Add(enemySpawner))
            {
                Debug.LogError($"중복 등록된 적 스포너가 있습니다: {enemySpawner.name}");
                return false;
            }
        }

        foreach (BAEnemySpawner enemySpawner in _enemySpawners)
        {
            enemySpawner.Cleared += OnSpawnerCleared;
            enemySpawner.RemainingEnemyCountChanged += OnRemainingEnemyCountChanged;
        }

        _isInitialized = true;
        RemainingEnemyCountChanged?.Invoke(RemainingEnemyCount);
        CheckStageCleared();
        return true;
    }

    public bool TryBindBattleResult(BABattleManager battleManager)
    {
        if (!_isInitialized)
        {
            Debug.LogError("스테이지 매니저가 초기화되지 않아 전투 결과를 바인딩할 수 없습니다.");
            return false;
        }

        if (battleManager == null)
        {
            Debug.LogError("BABattleManager가 없어 전투 결과를 바인딩할 수 없습니다.");
            return false;
        }

        if (!battleManager.IsInitialized)
        {
            Debug.LogError("BABattleManager가 초기화되지 않아 전투 결과를 바인딩할 수 없습니다.");
            return false;
        }

        if (_isBattleResultBound)
        {
            return true;
        }

        _battleManager = battleManager;
        _battleManager.UnitBound += OnUnitBound;

        if (_battleManager.TryGetFirstUnitViewModelByType(
                _heroUnitType,
                out BAUnitViewModel heroViewModel))
        {
            BindHeroViewModel(heroViewModel);
        }

        _isBattleResultBound = true;
        return true;
    }

    private void OnSpawnerCleared()
    {
        CheckStageCleared();
    }

    private void OnRemainingEnemyCountChanged(int remainingEnemyCount)
    {
        RemainingEnemyCountChanged?.Invoke(RemainingEnemyCount);
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
        if (_heroViewModel == heroViewModel)
        {
            return;
        }

        if (_heroViewModel != null)
        {
            _heroViewModel.Died -= OnHeroDied;
        }

        _heroViewModel = heroViewModel;
        _heroViewModel.Died += OnHeroDied;
    }

    private void OnHeroDied()
    {
        if (!_isBattleResultBound || _isStageCleared || _isStageFailed)
        {
            return;
        }

        _isStageFailed = true;
        Debug.Log("스테이지에 실패했습니다.");
        StageFailed?.Invoke();
    }

    private void CheckStageCleared()
    {
        if (!_isInitialized || _isStageCleared || _isStageFailed)
        {
            return;
        }

        foreach (BAEnemySpawner enemySpawner in _enemySpawners)
        {
            if (!enemySpawner.IsCleared)
            {
                return;
            }
        }

        _isStageCleared = true;
        Debug.Log("스테이지를 클리어했습니다.");
        StageCleared?.Invoke();
    }

    private void OnDestroy()
    {
        if (_battleManager != null)
        {
            _battleManager.UnitBound -= OnUnitBound;
        }

        if (_heroViewModel != null)
        {
            _heroViewModel.Died -= OnHeroDied;
        }

        if (_enemySpawners != null)
        {
            foreach (BAEnemySpawner enemySpawner in _enemySpawners)
            {
                if (enemySpawner != null)
                {
                    enemySpawner.Cleared -= OnSpawnerCleared;
                    enemySpawner.RemainingEnemyCountChanged -= OnRemainingEnemyCountChanged;
                }
            }
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
