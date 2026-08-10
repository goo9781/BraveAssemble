using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BADataManager : MonoBehaviour
{
    private const string _unitTableResourcePath = "Data/Tables/UnitTable";

    private readonly Dictionary<string, BAUnitData> _unitDataById = new Dictionary<string, BAUnitData>();

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

        ResourceRequest loadRequest = Resources.LoadAsync<TextAsset>(_unitTableResourcePath);
        yield return loadRequest;

        TextAsset unitTableAsset = loadRequest.asset as TextAsset;

        if (unitTableAsset == null)
        {
            Debug.LogError($"유닛 데이터 테이블을 불러오지 못했습니다: Resources/{_unitTableResourcePath}");
            _isInitializing = false;
            yield break;
        }

        BAUnitTableData unitTableData = null;

        try
        {
            unitTableData = JsonUtility.FromJson<BAUnitTableData>(unitTableAsset.text);
        }
        catch (Exception exception)
        {
            Debug.LogError($"유닛 데이터 테이블 역직렬화 중 오류가 발생했습니다: {exception.Message}");
            _isInitializing = false;
            yield break;
        }

        if (unitTableData == null || unitTableData.Units == null)
        {
            Debug.LogError("유닛 데이터 테이블을 역직렬화하지 못했거나 유닛 목록이 없습니다.");
            _isInitializing = false;
            yield break;
        }

        _unitDataById.Clear();

        foreach (BAUnitData unitData in unitTableData.Units)
        {
            if (unitData == null)
            {
                Debug.LogError("유닛 데이터 테이블에서 비어 있는 유닛 항목을 발견했습니다.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(unitData.Id))
            {
                Debug.LogError("유닛 데이터 테이블에서 ID가 비어 있는 유닛 항목을 발견했습니다.");
                continue;
            }

            if (_unitDataById.ContainsKey(unitData.Id))
            {
                Debug.LogError($"유닛 데이터 테이블에서 중복된 ID를 발견했습니다: {unitData.Id}");
                continue;
            }

            _unitDataById.Add(unitData.Id, unitData);
        }

        if (_unitDataById.Count == 0)
        {
            Debug.LogError("유효한 유닛 데이터가 없어 데이터 매니저 초기화에 실패했습니다.");
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
}
