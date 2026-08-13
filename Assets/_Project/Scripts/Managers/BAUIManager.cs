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
    private BABattleHudView _battleHudView;
    private BABattleHudViewModel _battleHudViewModel;
    private BAGameManager _gameManager;
    private bool _isBattleHudCommandsBound;

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

    public bool TryBindBattleHud(
        BABattleManager battleManager,
        BAStageManager stageManager,
        BASkillManager skillManager)
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
            new BABattleHudViewModel(battleManager, stageManager, skillManager);

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
        _battleHudViewModel.PauseRequested += OnPauseRequested;
        _battleHudViewModel.ResumeRequested += OnResumeRequested;
        _gameManager.StateChanged += OnGameStateChanged;
        _battleHudViewModel.UpdateGameState(_gameManager.CurrentState);
        _isBattleHudCommandsBound = true;
        return true;
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
        if (_battleHudView != null)
        {
            _battleHudView.Unbind();
        }

        if (_battleHudViewModel != null)
        {
            _battleHudViewModel.RestartRequested -= OnRestartRequested;
            _battleHudViewModel.QuitRequested -= OnQuitRequested;
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
