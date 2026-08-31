using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BAUIManager : MonoBehaviour
{
    [SerializeField] private Transform _screenRoot;
    [SerializeField] private GameObject _loadingUI;
    [SerializeField] private GameObject _startupLoadingUI;
    [SerializeField] private string _mainUIPrefabKey;
    [SerializeField] private string _battleHudPrefabKey;

    private bool _isInitialized;
    private bool _isInitializing;
    private bool _isBattleHudLoading;
    private BAAssetManager _assetManager;
    private BALoadingUIView _loadingUIView;
    private BALoadingUIView _startupLoadingUIView;
    private GameObject _mainUIInstance;
    private BAMainUIView _mainUIView;
    private GameObject _battleHudInstance;
    private BABattleHudView _battleHudView;
    private BABattleHudViewModel _battleHudViewModel;
    private BAGameManager _gameManager;
    private bool _isBattleHudCommandsBound;

    public static BAUIManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;

    public event Action StartGameRequested;
    public event Action QuitGameRequested;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        bool shouldSkipStartupLoadingUI = BAGameManager.ConsumeShouldSkipStartupLoadingUI();

        if (_startupLoadingUI != null)
        {
            _startupLoadingUIView = _startupLoadingUI.GetComponent<BALoadingUIView>();

            if (_startupLoadingUIView == null)
            {
                Debug.LogError("Startup Loading UI에서 BALoadingUIView 컴포넌트를 찾을 수 없습니다.");
            }
        }

        if (_loadingUI != null)
        {
            _loadingUIView = _loadingUI.GetComponent<BALoadingUIView>();

            if (_loadingUIView == null)
            {
                Debug.LogError("Loading UI에서 BALoadingUIView 컴포넌트를 찾을 수 없습니다.");
            }
        }

        if (shouldSkipStartupLoadingUI)
        {
            if (_startupLoadingUI != null)
            {
                _startupLoadingUI.SetActive(false);
            }

            if (_loadingUIView != null)
            {
                _loadingUIView.ShowRandomImage();
            }

            if (_loadingUI != null)
            {
                _loadingUI.SetActive(true);
            }
        }
        else
        {
            if (_startupLoadingUIView != null)
            {
                _startupLoadingUIView.ShowRandomImage();
            }

            if (_startupLoadingUI != null)
            {
                _startupLoadingUI.SetActive(true);
            }

            if (_loadingUI != null)
            {
                _loadingUI.SetActive(false);
            }
        }
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

        if (_screenRoot == null)
        {
            Debug.LogError("Screen Root가 설정되지 않아 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (_loadingUI == null)
        {
            Debug.LogError("Loading UI가 설정되지 않아 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (_loadingUIView == null)
        {
            Debug.LogError("BALoadingUIView가 없어 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (_startupLoadingUI == null)
        {
            Debug.LogError("Startup Loading UI가 설정되지 않아 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (_startupLoadingUIView == null)
        {
            Debug.LogError("Startup Loading UI의 BALoadingUIView가 없어 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (string.IsNullOrWhiteSpace(_mainUIPrefabKey))
        {
            Debug.LogError("Main UI Addressables 키가 비어 있어 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        if (string.IsNullOrWhiteSpace(_battleHudPrefabKey))
        {
            Debug.LogError("전투 HUD Addressables 키가 비어 있어 UI 매니저를 초기화할 수 없습니다.");
            _isInitializing = false;
            yield break;
        }

        _assetManager = assetManager;
        GameObject mainUIPrefab = null;

        yield return _assetManager.LoadPrefabAsync(
            _mainUIPrefabKey,
            prefab => mainUIPrefab = prefab);

        if (mainUIPrefab == null)
        {
            Debug.LogError($"Main UI 프리팹을 불러오지 못했습니다: {_mainUIPrefabKey}");
            _assetManager = null;
            _isInitializing = false;
            yield break;
        }

        if (_mainUIInstance == null)
        {
            _mainUIInstance = Instantiate(mainUIPrefab, _screenRoot, false);
        }

        _mainUIView = _mainUIInstance.GetComponent<BAMainUIView>();

        if (_mainUIView == null)
        {
            Debug.LogError("Main UI 루트에서 BAMainUIView 컴포넌트를 찾을 수 없습니다.");
            _assetManager = null;
            _isInitializing = false;
            yield break;
        }

        _mainUIView.StartRequested -= OnMainUIStartRequested;
        _mainUIView.QuitRequested -= OnMainUIQuitRequested;
        _mainUIView.StartRequested += OnMainUIStartRequested;
        _mainUIView.QuitRequested += OnMainUIQuitRequested;

        if (!_mainUIView.Bind())
        {
            _mainUIView.StartRequested -= OnMainUIStartRequested;
            _mainUIView.QuitRequested -= OnMainUIQuitRequested;
            _assetManager = null;
            _isInitializing = false;
            yield break;
        }

        _isInitialized = true;
        _isInitializing = false;
    }

    public IEnumerator LoadBattleHudAsync()
    {
        if (!_isInitialized || _assetManager == null || !_assetManager.IsInitialized)
        {
            Debug.LogError("UI 매니저 또는 BAAssetManager가 초기화되지 않아 전투 HUD를 불러올 수 없습니다.");
            yield break;
        }

        if (_battleHudInstance != null)
        {
            yield break;
        }

        if (_isBattleHudLoading)
        {
            while (_isBattleHudLoading)
            {
                yield return null;
            }

            yield break;
        }

        _isBattleHudLoading = true;
        GameObject battleHudPrefab = null;

        yield return _assetManager.LoadPrefabAsync(
            _battleHudPrefabKey,
            prefab => battleHudPrefab = prefab);

        if (battleHudPrefab == null)
        {
            Debug.LogError($"전투 HUD 프리팹을 불러오지 못했습니다: {_battleHudPrefabKey}");
            _isBattleHudLoading = false;
            yield break;
        }

        if (_battleHudInstance == null)
        {
            _battleHudInstance = Instantiate(battleHudPrefab, _screenRoot, false);
            _battleHudInstance.SetActive(false);
        }

        _isBattleHudLoading = false;
    }

    public bool TryGetBattleHud(out GameObject battleHudInstance)
    {
        battleHudInstance = _battleHudInstance;
        return _isInitialized && battleHudInstance != null;
    }

    public bool TryBindBattleHud(
        BABattleManager battleManager,
        BAStageManager stageManager,
        BASkillManager skillManager,
        BAAssembleManager assembleManager,
        BASupportManager supportManager)
    {
        if (!_isInitialized)
        {
            Debug.LogError("UI 매니저가 초기화되지 않아 전투 HUD를 바인딩할 수 없습니다.");
            return false;
        }

        if (_battleHudInstance == null)
        {
            Debug.LogError("생성된 전투 HUD 인스턴스가 없어 바인딩할 수 없습니다.");
            return false;
        }

        if (battleManager == null || !battleManager.IsInitialized)
        {
            Debug.LogError("BABattleManager가 없거나 초기화되지 않아 전투 HUD를 바인딩할 수 없습니다.");
            return false;
        }

        if (stageManager == null || !stageManager.IsInitialized)
        {
            Debug.LogError("BAStageManager가 없거나 초기화되지 않아 전투 HUD를 바인딩할 수 없습니다.");
            return false;
        }

        if (skillManager == null || !skillManager.IsInitialized)
        {
            Debug.LogError("BASkillManager가 없거나 초기화되지 않아 전투 HUD를 바인딩할 수 없습니다.");
            return false;
        }

        if (assembleManager == null || !assembleManager.IsInitialized)
        {
            Debug.LogError("BAAssembleManager가 없거나 초기화되지 않아 전투 HUD를 바인딩할 수 없습니다.");
            return false;
        }

        if (supportManager == null || !supportManager.IsInitialized)
        {
            Debug.LogError("BASupportManager가 없거나 초기화되지 않아 전투 HUD를 바인딩할 수 없습니다.");
            return false;
        }

        if (_battleHudView != null && _battleHudViewModel != null)
        {
            return true;
        }

        BABattleHudView battleHudView = _battleHudInstance.GetComponent<BABattleHudView>();

        if (battleHudView == null)
        {
            Debug.LogError("전투 HUD 루트에서 BABattleHudView 컴포넌트를 찾을 수 없습니다.");
            return false;
        }

        BABattleHudViewModel battleHudViewModel =
            new BABattleHudViewModel(
                battleManager,
                stageManager,
                skillManager,
                assembleManager,
                supportManager);

        if (!battleHudView.Bind(battleHudViewModel))
        {
            battleHudViewModel.Dispose();
            return false;
        }

        _battleHudView = battleHudView;
        _battleHudViewModel = battleHudViewModel;
        return true;
    }

    public bool TryBindBattleHudCommands(BAGameManager gameManager)
    {
        if (!_isInitialized)
        {
            Debug.LogError("UI 매니저가 초기화되지 않아 전투 HUD 명령을 바인딩할 수 없습니다.");
            return false;
        }

        if (_battleHudViewModel == null)
        {
            Debug.LogError("전투 HUD ViewModel이 없어 명령을 바인딩할 수 없습니다.");
            return false;
        }

        if (gameManager == null)
        {
            Debug.LogError("BAGameManager가 없어 전투 HUD 명령을 바인딩할 수 없습니다.");
            return false;
        }

        if (_isBattleHudCommandsBound)
        {
            return true;
        }

        _gameManager = gameManager;
        _battleHudViewModel.RestartRequested += OnRestartRequested;
        _battleHudViewModel.QuitRequested += OnQuitRequested;
        _battleHudViewModel.MainRequested += OnMainRequested;
        _battleHudViewModel.PauseRequested += OnPauseRequested;
        _battleHudViewModel.ResumeRequested += OnResumeRequested;
        _gameManager.StateChanged += OnGameStateChanged;
        _battleHudViewModel.UpdateGameState(_gameManager.CurrentState);
        _isBattleHudCommandsBound = true;
        return true;
    }

    public void ShowMainUI()
    {
        if (_mainUIInstance == null)
        {
            Debug.LogError("Main UI가 준비되지 않아 표시할 수 없습니다.");
            return;
        }

        _mainUIInstance.SetActive(true);

        if (_battleHudInstance != null)
        {
            _battleHudInstance.SetActive(false);
        }

        if (_loadingUI != null)
        {
            _loadingUI.SetActive(false);
        }

        if (_startupLoadingUI != null)
        {
            _startupLoadingUI.SetActive(false);
        }
    }

    public void ShowLoadingUI()
    {
        if (_startupLoadingUI != null)
        {
            _startupLoadingUI.SetActive(false);
        }

        if (_loadingUI == null)
        {
            Debug.LogError("Loading UI가 설정되지 않아 표시할 수 없습니다.");
            return;
        }

        if (_loadingUIView == null)
        {
            Debug.LogError("BALoadingUIView가 없어 Loading UI를 표시할 수 없습니다.");
            return;
        }

        if (!_loadingUI.activeSelf)
        {
            _loadingUIView.ShowRandomImage();
        }

        _loadingUI.SetActive(true);
        _loadingUI.transform.SetAsLastSibling();
    }

    public void ShowBattleHud()
    {
        if (_mainUIInstance == null)
        {
            Debug.LogError("Main UI가 준비되지 않아 전투 화면으로 전환할 수 없습니다.");
            return;
        }

        if (_battleHudInstance == null)
        {
            Debug.LogError("전투 HUD가 준비되지 않아 표시할 수 없습니다.");
            return;
        }

        _mainUIInstance.SetActive(false);
        _battleHudInstance.SetActive(true);

        if (_loadingUI != null)
        {
            _loadingUI.SetActive(false);
        }

        if (_startupLoadingUI != null)
        {
            _startupLoadingUI.SetActive(false);
        }
    }

    private void OnMainUIStartRequested()
    {
        StartGameRequested?.Invoke();
    }

    private void OnMainUIQuitRequested()
    {
        QuitGameRequested?.Invoke();
    }

    private void OnRestartRequested()
    {
        if (_gameManager != null)
        {
            _gameManager.RestartGame();
        }
    }

    private void OnQuitRequested()
    {
        if (_gameManager != null)
        {
            _gameManager.QuitGame();
        }
    }

    private void OnMainRequested()
    {
        if (_gameManager != null)
        {
            _gameManager.ReturnToMain();
        }
    }

    private void OnPauseRequested()
    {
        if (_gameManager != null)
        {
            _gameManager.PauseGame();
        }
    }

    private void OnResumeRequested()
    {
        if (_gameManager != null)
        {
            _gameManager.ResumeGame();
        }
    }

    private void OnGameStateChanged(BAGameState gameState)
    {
        if (_battleHudViewModel != null)
        {
            _battleHudViewModel.UpdateGameState(gameState);
        }
    }

    private void OnDestroy()
    {
        if (_mainUIView != null)
        {
            _mainUIView.StartRequested -= OnMainUIStartRequested;
            _mainUIView.QuitRequested -= OnMainUIQuitRequested;
            _mainUIView.Unbind();
        }

        StartGameRequested = null;
        QuitGameRequested = null;
        _mainUIView = null;

        if (_battleHudView != null)
        {
            _battleHudView.Unbind();
        }

        if (_battleHudViewModel != null)
        {
            _battleHudViewModel.RestartRequested -= OnRestartRequested;
            _battleHudViewModel.QuitRequested -= OnQuitRequested;
            _battleHudViewModel.MainRequested -= OnMainRequested;
            _battleHudViewModel.PauseRequested -= OnPauseRequested;
            _battleHudViewModel.ResumeRequested -= OnResumeRequested;
        }

        if (_gameManager != null)
        {
            _gameManager.StateChanged -= OnGameStateChanged;
        }

        _gameManager = null;
        _isBattleHudCommandsBound = false;
        _battleHudViewModel?.Dispose();
        _battleHudView = null;
        _battleHudViewModel = null;

        if (_mainUIInstance != null)
        {
            Destroy(_mainUIInstance);
            _mainUIInstance = null;
        }

        if (_battleHudInstance != null)
        {
            Destroy(_battleHudInstance);
            _battleHudInstance = null;
        }

        _loadingUIView = null;
        _startupLoadingUIView = null;
        _loadingUI = null;
        _assetManager = null;
        _isInitialized = false;
        _isInitializing = false;
        _isBattleHudLoading = false;

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
