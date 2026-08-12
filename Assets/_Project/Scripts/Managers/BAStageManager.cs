using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BAStageManager : MonoBehaviour
{
    [SerializeField] private List<BAEnemySpawner> _enemySpawners = new List<BAEnemySpawner>();

    private bool _isInitialized;
    private bool _isStageCleared;

    public static BAStageManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;
    public bool IsStageCleared => _isStageCleared;

    public event Action StageCleared;

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
        }

        _isInitialized = true;
        CheckStageCleared();
        return true;
    }

    private void OnSpawnerCleared()
    {
        CheckStageCleared();
    }

    private void CheckStageCleared()
    {
        if (!_isInitialized || _isStageCleared)
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
        if (_enemySpawners != null)
        {
            foreach (BAEnemySpawner enemySpawner in _enemySpawners)
            {
                if (enemySpawner != null)
                {
                    enemySpawner.Cleared -= OnSpawnerCleared;
                }
            }
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
