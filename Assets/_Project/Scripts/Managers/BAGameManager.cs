using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BAGameManager : MonoBehaviour
{
    [SerializeField] private BADataManager _dataManager;
    [SerializeField] private BAAssetManager _assetManager;
    [SerializeField] private BAPoolManager _poolManager;
    [SerializeField] private BABattleManager _battleManager;
    [SerializeField] private BAStageManager _stageManager;

    private bool _isInitialized;

    public static BAGameManager Instance { get; private set; }

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

    private void Start()
    {
        StartCoroutine(InitializeGameAsync());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private IEnumerator InitializeGameAsync()
    {
        if (_dataManager == null)
        {
            Debug.LogError("BADataManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        yield return _dataManager.InitializeAsync();

        if (!_dataManager.IsInitialized)
        {
            Debug.LogError("데이터 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (_assetManager == null)
        {
            Debug.LogError("BAAssetManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        yield return _assetManager.InitializeAsync();

        if (!_assetManager.IsInitialized)
        {
            Debug.LogError("에셋 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (_poolManager == null)
        {
            Debug.LogError("BAPoolManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        _poolManager.Initialize();

        if (!_poolManager.IsInitialized)
        {
            Debug.LogError("풀 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (_battleManager == null)
        {
            Debug.LogError("BABattleManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        if (!_battleManager.Initialize(_dataManager))
        {
            Debug.LogError("전투 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (_stageManager == null)
        {
            Debug.LogError("BAStageManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        if (!_stageManager.Initialize())
        {
            Debug.LogError("스테이지 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        _isInitialized = true;
        Debug.Log("게임 초기화를 완료했습니다.");
    }
}
