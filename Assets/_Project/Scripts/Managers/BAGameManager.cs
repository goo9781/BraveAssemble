using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BAGameManager : MonoBehaviour
{
    [SerializeField] private BADataManager _dataManager;
    [SerializeField] private BABattleManager _battleManager;

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

        _isInitialized = true;
        Debug.Log("게임 초기화를 완료했습니다.");
    }
}
