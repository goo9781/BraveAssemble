using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BADataManager : MonoBehaviour
{
    private const string _unitTableResourcePath = "JsonOutput/UnitTable";
    private const string _enemySpawnTableResourcePath = "JsonOutput/EnemySpawnTable";
    private const string _skillTableResourcePath = "JsonOutput/SkillTable";
    private const string _assembleTableResourcePath = "JsonOutput/AssembleTable";

    [Serializable]
    private class SerializationWrapper<T> where T : BAGameDataBase
    {
        public List<T> items;
    }

    private readonly Dictionary<string, BAUnitData> _unitDataById = new Dictionary<string, BAUnitData>();
    private readonly Dictionary<string, BAEnemySpawnData> _enemySpawnDataById =
        new Dictionary<string, BAEnemySpawnData>();
    private readonly Dictionary<string, BASkillData> _skillDataById =
        new Dictionary<string, BASkillData>();
    private readonly Dictionary<string, BAAssembleData> _assembleDataById =
        new Dictionary<string, BAAssembleData>();

    private bool _isInitialized;
    private bool _isInitializing;

    public static BADataManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public IEnumerator InitializeAsync()
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

            if (_isInitialized)
            {
                yield break;
            }
        }

        _isInitializing = true;
        bool unitTableLoaded = false;
        bool enemySpawnTableLoaded = false;
        bool skillTableLoaded = false;
        bool assembleTableLoaded = false;

        yield return LoadTableAsync(
            _unitTableResourcePath,
            _unitDataById,
            succeeded => unitTableLoaded = succeeded);

        if (!unitTableLoaded)
        {
            _isInitializing = false;
            yield break;
        }

        yield return LoadTableAsync(
            _enemySpawnTableResourcePath,
            _enemySpawnDataById,
            succeeded => enemySpawnTableLoaded = succeeded);

        if (!enemySpawnTableLoaded)
        {
            _isInitializing = false;
            yield break;
        }

        yield return LoadTableAsync(
            _skillTableResourcePath,
            _skillDataById,
            succeeded => skillTableLoaded = succeeded);

        if (!skillTableLoaded)
        {
            _isInitializing = false;
            yield break;
        }

        yield return LoadTableAsync(
            _assembleTableResourcePath,
            _assembleDataById,
            succeeded => assembleTableLoaded = succeeded);

        if (!assembleTableLoaded)
        {
            _isInitializing = false;
            yield break;
        }

        _isInitialized = true;
        _isInitializing = false;
    }

    public bool TryGetUnitData(string id, out BAUnitData unitData)
    {
        unitData = null;

        if (!_isInitialized || string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return _unitDataById.TryGetValue(id, out unitData);
    }

    public bool TryGetEnemySpawnData(string id, out BAEnemySpawnData enemySpawnData)
    {
        enemySpawnData = null;

        if (!_isInitialized || string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return _enemySpawnDataById.TryGetValue(id, out enemySpawnData);
    }

    public bool TryGetSkillData(string id, out BASkillData skillData)
    {
        skillData = null;

        if (!_isInitialized || string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return _skillDataById.TryGetValue(id, out skillData);
    }

    public bool TryGetAssembleData(string id, out BAAssembleData assembleData)
    {
        assembleData = null;

        if (!_isInitialized || string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return _assembleDataById.TryGetValue(id, out assembleData);
    }

    private IEnumerator LoadTableAsync<T>(
        string resourcePath,
        Dictionary<string, T> targetDictionary,
        Action<bool> onCompleted) where T : BAGameDataBase
    {
        ResourceRequest loadRequest = Resources.LoadAsync<TextAsset>(resourcePath);
        yield return loadRequest;

        TextAsset tableAsset = loadRequest.asset as TextAsset;

        if (tableAsset == null)
        {
            Debug.LogError($"데이터 테이블을 불러오지 못했습니다: Resources/{resourcePath}");
            onCompleted?.Invoke(false);
            yield break;
        }

        try
        {
            string wrappedJson = "{\"items\":" + tableAsset.text + "}";
            SerializationWrapper<T> wrapper =
                JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if (wrapper == null || wrapper.items == null)
            {
                Debug.LogError($"데이터 테이블을 역직렬화하지 못했거나 데이터 목록이 없습니다: {resourcePath}");
                onCompleted?.Invoke(false);
                yield break;
            }

            targetDictionary.Clear();

            foreach (T data in wrapper.items)
            {
                if (data == null)
                {
                    Debug.LogError($"데이터 테이블에서 비어 있는 항목을 발견했습니다: {resourcePath}");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(data.ID))
                {
                    Debug.LogError($"데이터 테이블에서 ID가 비어 있는 항목을 발견했습니다: {resourcePath}");
                    continue;
                }

                if (targetDictionary.ContainsKey(data.ID))
                {
                    Debug.LogError($"데이터 테이블에서 중복된 ID를 발견했습니다: {resourcePath}, ID: {data.ID}");
                    continue;
                }

                targetDictionary.Add(data.ID, data);
            }

            if (targetDictionary.Count == 0)
            {
                Debug.LogError($"데이터 테이블에 유효한 데이터가 없습니다: {resourcePath}");
                onCompleted?.Invoke(false);
                yield break;
            }

            onCompleted?.Invoke(true);
        }
        catch (Exception exception)
        {
            Debug.LogError($"데이터 테이블 처리 중 오류가 발생했습니다: {resourcePath}, 오류: {exception.Message}");
            onCompleted?.Invoke(false);
        }
    }
}
