using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BAUIManager : MonoBehaviour
{
    [SerializeField] private Transform _uiRoot;
    [SerializeField] private string _battleHudPrefabKey;

    private bool _isInitialized;
    private bool _isInitializing;
    private GameObject _battleHudInstance;

    public static BAUIManager Instance { get; private set; }

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

    public IEnumerator InitializeAsync(BAAssetManager assetManager)
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

        if (assetManager == null)
        {
            Debug.LogError("BAAssetManager가 없어 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (!assetManager.IsInitialized)
        {
            Debug.LogError("BAAssetManager가 초기화되지 않아 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (_uiRoot == null)
        {
            Debug.LogError("UI Root가 설정되지 않아 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (string.IsNullOrWhiteSpace(_battleHudPrefabKey))
        {
            Debug.LogError("전투 HUD Addressables 키가 비어 있어 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        GameObject battleHudPrefab = null;

        yield return assetManager.LoadPrefabAsync(
            _battleHudPrefabKey,
            prefab => battleHudPrefab = prefab);

        if (battleHudPrefab == null)
        {
            Debug.LogError($"전투 HUD 프리팹을 불러오지 못했습니다: {_battleHudPrefabKey}");
            _isInitializing = false;
            yield break;
        }

        if (_battleHudInstance == null)
        {
            _battleHudInstance = Instantiate(battleHudPrefab, _uiRoot, false);
        }

        _isInitialized = true;
        _isInitializing = false;
    }

    public bool TryGetBattleHud(out GameObject battleHudInstance)
    {
        battleHudInstance = _battleHudInstance;
        return _isInitialized && battleHudInstance != null;
    }

    private void OnDestroy()
    {
        if (_battleHudInstance != null)
        {
            Destroy(_battleHudInstance);
            _battleHudInstance = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
