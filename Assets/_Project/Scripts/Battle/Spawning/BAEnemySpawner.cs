using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BAEnemySpawner : MonoBehaviour
{
    private const float _initializationTimeout = 10f;

    [SerializeField] private string _spawnDataId;
    [SerializeField] private Camera _battleCamera;
    [SerializeField, Min(0f)] private float _spawnVisualHalfWidth = 1.76f;
    [SerializeField, Min(0f)] private float _spawnOutsideMargin = 0.3f;

    private readonly HashSet<GameObject> _spawnedEnemies = new HashSet<GameObject>();

    private BAEnemySpawnData _spawnData;
    private bool _isInitialized;
    private bool _isSpawnCompleted;
    private bool _isCleared;
    private int _spawnedCount;
    private int _defeatedCount;

    public bool IsInitialized => _isInitialized;
    public bool IsSpawnCompleted => _isSpawnCompleted;
    public bool IsCleared => _isCleared;
    public int SpawnedCount => _spawnedCount;
    public int RemainingEnemyCount =>
        _spawnData == null ? 0 : Mathf.Max(0, _spawnData.TotalSpawnCount - _defeatedCount);

    public event Action SpawnCompleted;
    public event Action Cleared;
    public event Action<int> RemainingEnemyCountChanged;

    private IEnumerator Start()
    {
        float elapsedTime = 0f;

        while ((BAGameManager.Instance == null || !BAGameManager.Instance.IsInitialized) &&
               elapsedTime < _initializationTimeout)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (BAGameManager.Instance == null || !BAGameManager.Instance.IsInitialized)
        {
            Debug.LogError("게임 매니저 초기화 대기 시간이 초과되어 적 스포너 초기화를 중단합니다.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(_spawnDataId))
        {
            Debug.LogError("적 스폰 규칙 ID가 설정되지 않아 초기화를 중단합니다.");
            yield break;
        }

        if (BADataManager.Instance == null ||
            !BADataManager.Instance.TryGetEnemySpawnData(_spawnDataId, out _spawnData))
        {
            Debug.LogError($"적 스폰 데이터를 찾을 수 없습니다: {_spawnDataId}");
            yield break;
        }

        if (!BADataManager.Instance.TryGetUnitData(_spawnData.UnitID, out BAUnitData unitData))
        {
            Debug.LogError($"적 유닛 데이터를 찾을 수 없습니다: {_spawnData.UnitID}");
            yield break;
        }

        if (_spawnData.SpawnInterval <= 0f ||
            _spawnData.InitialPoolSize < 0 ||
            _spawnData.MaxAliveCount <= 0 ||
            _spawnData.TotalSpawnCount <= 0)
        {
            Debug.LogError($"적 스폰 데이터의 수치가 유효하지 않습니다: {_spawnDataId}");
            yield break;
        }

        GameObject enemyPrefab = null;

        if (BAAssetManager.Instance == null)
        {
            Debug.LogError("BAAssetManager가 없어 적 프리팹을 불러올 수 없습니다.");
            yield break;
        }

        yield return BAAssetManager.Instance.LoadPrefabAsync(
            unitData.PrefabKey,
            prefab => enemyPrefab = prefab);

        if (enemyPrefab == null)
        {
            Debug.LogError($"적 프리팹을 불러오지 못했습니다: {unitData.PrefabKey}");
            yield break;
        }

        if (enemyPrefab.GetComponent<BAUnitCombatController>() == null ||
            enemyPrefab.GetComponent<BAUnitView>() == null)
        {
            Debug.LogError($"적 프리팹 루트에 필요한 전투 컴포넌트가 없습니다: {unitData.PrefabKey}");
            yield break;
        }

        if (BAPoolManager.Instance == null ||
            !BAPoolManager.Instance.RegisterPool(
                _spawnData.UnitID,
                enemyPrefab,
                _spawnData.InitialPoolSize))
        {
            Debug.LogError($"적 오브젝트 풀 등록에 실패했습니다: {_spawnData.UnitID}");
            yield break;
        }

        _isInitialized = true;
        RemainingEnemyCountChanged?.Invoke(RemainingEnemyCount);
        StartCoroutine(SpawnEnemiesAsync());
    }

    private IEnumerator SpawnEnemiesAsync()
    {
        yield return WaitForPlayingDuration(_spawnData.StartDelay);

        while (_spawnedCount < _spawnData.TotalSpawnCount)
        {
            while (!IsGamePlaying())
            {
                yield return null;
            }

            if (CountActiveEnemies() >= _spawnData.MaxAliveCount)
            {
                yield return null;
                continue;
            }

            Vector3 spawnPosition = transform.position;

            if (_battleCamera != null)
            {
                float spawnDistance = Mathf.Abs(
                    spawnPosition.z - _battleCamera.transform.position.z);
                Vector3 rightViewportPosition = _battleCamera.ViewportToWorldPoint(
                    new Vector3(1f, 0.5f, spawnDistance));

                spawnPosition.x =
                    rightViewportPosition.x +
                    Mathf.Max(0f, _spawnVisualHalfWidth) +
                    Mathf.Max(0f, _spawnOutsideMargin);
            }

            GameObject enemy = BAPoolManager.Instance.Spawn(
                _spawnData.UnitID,
                spawnPosition,
                transform.rotation);

            if (enemy != null)
            {
                if (_spawnedEnemies.Add(enemy))
                {
                    BAUnitView unitView = enemy.GetComponent<BAUnitView>();

                    if (unitView != null)
                    {
                        unitView.Died += OnEnemyDied;
                    }
                }

                _spawnedCount++;
            }

            yield return WaitForPlayingDuration(_spawnData.SpawnInterval);
        }

        _isSpawnCompleted = true;
        SpawnCompleted?.Invoke();

        while (CountActiveEnemies() > 0)
        {
            yield return null;
        }

        _isCleared = true;
        Cleared?.Invoke();
    }

    private IEnumerator WaitForPlayingDuration(float duration)
    {
        float elapsedTime = 0f;

        while (!IsGamePlaying())
        {
            yield return null;
        }

        while (elapsedTime < duration)
        {
            yield return null;

            if (!IsGamePlaying())
            {
                continue;
            }

            elapsedTime += Time.deltaTime;
        }
    }

    private bool IsGamePlaying()
    {
        return BAGameManager.Instance != null &&
               BAGameManager.Instance.IsInitialized &&
               BAGameManager.Instance.CurrentState == BAGameState.Playing;
    }

    private void OnEnemyDied()
    {
        if (!_isInitialized || _spawnData == null)
        {
            return;
        }

        _defeatedCount = Mathf.Min(_defeatedCount + 1, _spawnData.TotalSpawnCount);
        RemainingEnemyCountChanged?.Invoke(RemainingEnemyCount);
    }

    private int CountActiveEnemies()
    {
        int activeEnemyCount = 0;

        foreach (GameObject enemy in _spawnedEnemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                activeEnemyCount++;
            }
        }

        return activeEnemyCount;
    }

    private void OnDestroy()
    {
        foreach (GameObject enemy in _spawnedEnemies)
        {
            if (enemy == null)
            {
                continue;
            }

            BAUnitView unitView = enemy.GetComponent<BAUnitView>();

            if (unitView != null)
            {
                unitView.Died -= OnEnemyDied;
            }
        }
    }
}
