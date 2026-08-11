using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BABattleManager : MonoBehaviour
{
    private readonly Dictionary<BAUnitView, BAUnitViewModel> _boundUnits =
        new Dictionary<BAUnitView, BAUnitViewModel>();

    private BADataManager _dataManager;
    private bool _isInitialized;

    public static BABattleManager Instance { get; private set; }

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

    public bool Initialize(BADataManager dataManager)
    {
        if (dataManager == null)
        {
            Debug.LogError("BADataManager가 없어 전투 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (!dataManager.IsInitialized)
        {
            Debug.LogError("BADataManager가 초기화되지 않아 전투 매니저를 초기화할 수 없습니다.");
            return false;
        }

        _dataManager = dataManager;
        _isInitialized = true;
        return true;
    }

    public bool TryBindUnit(string unitId, BAUnitView unitView)
    {
        if (!_isInitialized || string.IsNullOrWhiteSpace(unitId) || unitView == null)
        {
            return false;
        }

        if (!_dataManager.TryGetUnitData(unitId, out BAUnitData unitData))
        {
            Debug.LogError($"유닛 데이터를 찾을 수 없습니다: {unitId}");
            return false;
        }

        if (_boundUnits.ContainsKey(unitView))
        {
            ReleaseUnit(unitView);
        }

        BAUnitModel unitModel = new BAUnitModel(unitData);
        BAUnitViewModel unitViewModel = new BAUnitViewModel(unitModel);

        unitView.Bind(unitViewModel);
        unitView.ResetState();
        _boundUnits.Add(unitView, unitViewModel);

        return true;
    }

    public void ReleaseUnit(BAUnitView unitView)
    {
        if (unitView == null)
        {
            return;
        }

        unitView.Unbind();

        if (_boundUnits.TryGetValue(unitView, out BAUnitViewModel unitViewModel))
        {
            unitViewModel.Dispose();
            _boundUnits.Remove(unitView);
        }

        unitView.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<BAUnitView, BAUnitViewModel> boundUnit in _boundUnits)
        {
            if (boundUnit.Key != null)
            {
                boundUnit.Key.Unbind();
            }

            boundUnit.Value?.Dispose();
        }

        _boundUnits.Clear();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
