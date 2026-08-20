using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BAGameState
{
    None = 0,
    Initializing,
    Playing,
    Paused,
    StageCleared,
    StageFailed
}

[DisallowMultipleComponent]
public class BAGameManager : MonoBehaviour
{
    [SerializeField] private BADataManager _dataManager;
    [SerializeField] private BAAssetManager _assetManager;
    [SerializeField] private BAPoolManager _poolManager;
    [SerializeField] private BABattleManager _battleManager;
    [SerializeField] private BAStageManager _stageManager;
    [SerializeField] private BASkillManager _skillManager;
    [SerializeField] private BAAssembleManager _assembleManager;
    [SerializeField] private BASupportManager _supportManager;
    [SerializeField] private BAUIManager _uiManager;

    private bool _isInitialized;
    private bool _isRestarting;
    private BAGameState _currentState = BAGameState.None;

    public static BAGameManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;
    public BAGameState CurrentState => _currentState;

    public event Action<BAGameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        ChangeState(BAGameState.Initializing);
        StartCoroutine(InitializeGameAsync());
    }

    public void RestartGame()
    {
        if (!_isInitialized)
        {
            Debug.LogError("게임이 초기화되지 않아 재시작할 수 없습니다.");
            return;
        }

        if (_isRestarting)
        {
            return;
        }

        StartCoroutine(RestartGameAsync());
    }

    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
        Application.Quit();
    }

    public void PauseGame()
    {
        if (!_isInitialized || _currentState != BAGameState.Playing)
        {
            return;
        }

        Time.timeScale = 0f;
        ChangeState(BAGameState.Paused);
    }

    public void ResumeGame()
    {
        if (_currentState != BAGameState.Paused)
        {
            return;
        }

        Time.timeScale = 1f;
        ChangeState(BAGameState.Playing);
    }

    private void OnDestroy()
    {
        if (_stageManager != null)
        {
            _stageManager.StageCleared -= OnStageCleared;
            _stageManager.StageFailed -= OnStageFailed;
        }

        if (Instance == this)
        {
            Time.timeScale = 1f;
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

        if (!_stageManager.TryBindBattleResult(_battleManager))
        {
            Debug.LogError("전투 결과 바인딩에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        _stageManager.StageCleared += OnStageCleared;
        _stageManager.StageFailed += OnStageFailed;

        if (_assembleManager == null)
        {
            Debug.LogError("BAAssembleManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        if (!_assembleManager.Initialize(
                _dataManager,
                _battleManager,
                _stageManager))
        {
            Debug.LogError("합체 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (_skillManager == null)
        {
            Debug.LogError("BASkillManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        if (!_skillManager.Initialize(
                _dataManager,
                _battleManager,
                _stageManager,
                _assembleManager))
        {
            Debug.LogError("스킬 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (_supportManager == null)
        {
            Debug.LogError("BASupportManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        yield return _supportManager.InitializeAsync(
            _dataManager,
            _assetManager,
            _poolManager,
            _battleManager,
            _stageManager,
            _assembleManager);

        if (!_supportManager.IsInitialized)
        {
            Debug.LogError("서포트 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (_uiManager == null)
        {
            Debug.LogError("BAUIManager 참조가 설정되지 않아 게임 초기화를 중단합니다.");
            yield break;
        }

        yield return _uiManager.InitializeAsync(_assetManager);

        if (!_uiManager.IsInitialized)
        {
            Debug.LogError("UI 매니저 초기화에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (!_uiManager.TryBindBattleHud(
                _battleManager,
                _stageManager,
                _skillManager,
                _assembleManager,
                _supportManager))
        {
            Debug.LogError("전투 HUD 바인딩에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        if (!_uiManager.TryBindBattleHudCommands(this))
        {
            Debug.LogError("전투 HUD 명령 바인딩에 실패하여 게임 초기화를 중단합니다.");
            yield break;
        }

        _isInitialized = true;
        ChangeState(BAGameState.Playing);
        Debug.Log("게임 초기화를 완료했습니다.");
    }

    private IEnumerator RestartGameAsync()
    {
        _isRestarting = true;
        ChangeState(BAGameState.Initializing);
        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.buildIndex < 0)
        {
            Debug.LogError("현재 씬이 빌드 설정에 등록되지 않아 재시작할 수 없습니다.");
            _isRestarting = false;
            yield break;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
            activeScene.buildIndex,
            LoadSceneMode.Single);

        if (loadOperation == null)
        {
            Debug.LogError("현재 씬의 비동기 로드를 시작하지 못했습니다.");
            _isRestarting = false;
            yield break;
        }

        yield return loadOperation;
    }

    private void ChangeState(BAGameState nextState)
    {
        if (_currentState == nextState)
        {
            return;
        }

        _currentState = nextState;
        StateChanged?.Invoke(_currentState);
    }

    private void OnStageCleared()
    {
        if (_currentState == BAGameState.StageCleared ||
            _currentState == BAGameState.StageFailed)
        {
            return;
        }

        Time.timeScale = 0f;
        ChangeState(BAGameState.StageCleared);
    }

    private void OnStageFailed()
    {
        if (_currentState == BAGameState.StageCleared ||
            _currentState == BAGameState.StageFailed)
        {
            return;
        }

        Time.timeScale = 0f;
        ChangeState(BAGameState.StageFailed);
    }
}
