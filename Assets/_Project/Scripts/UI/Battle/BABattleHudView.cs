using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BABattleHudView : MonoBehaviour
{
    [SerializeField] private Slider _heroHpSlider;
    [SerializeField] private TMP_Text _heroHpText;
    [SerializeField] private TMP_Text _remainingEnemyText;
    [SerializeField] private GameObject _stageClearPanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _stageClearRestartButton;
    [SerializeField] private Button _stageClearQuitButton;
    [SerializeField] private Button _gameOverRestartButton;
    [SerializeField] private Button _gameOverQuitButton;

    private BABattleHudViewModel _viewModel;

    public bool Bind(BABattleHudViewModel viewModel)
    {
        if (viewModel == null)
        {
            Debug.LogError("전투 HUD ViewModel이 없어 HUD를 바인딩할 수 없습니다.");
            return false;
        }

        if (_heroHpSlider == null ||
            _heroHpText == null ||
            _remainingEnemyText == null ||
            _stageClearPanel == null ||
            _gameOverPanel == null ||
            _stageClearRestartButton == null ||
            _stageClearQuitButton == null ||
            _gameOverRestartButton == null ||
            _gameOverQuitButton == null)
        {
            Debug.LogError("전투 HUD의 Inspector 참조가 모두 설정되지 않았습니다.");
            return false;
        }

        Unbind();

        _viewModel = viewModel;
        _viewModel.HeroHealthChanged += OnHeroHealthChanged;
        _viewModel.RemainingEnemyCountChanged += OnRemainingEnemyCountChanged;
        _viewModel.StageCleared += OnStageCleared;
        _viewModel.StageFailed += OnStageFailed;

        _stageClearRestartButton.onClick.AddListener(OnRestartButtonClicked);
        _stageClearQuitButton.onClick.AddListener(OnQuitButtonClicked);
        _gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
        _gameOverQuitButton.onClick.AddListener(OnQuitButtonClicked);

        UpdateHeroHealth(_viewModel.HeroCurrentHealth, _viewModel.HeroMaxHealth);
        UpdateRemainingEnemyCount(_viewModel.RemainingEnemyCount);
        _stageClearPanel.SetActive(_viewModel.IsStageCleared);
        _gameOverPanel.SetActive(_viewModel.IsStageFailed);

        return true;
    }

    public void Unbind()
    {
        if (_stageClearRestartButton != null)
        {
            _stageClearRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (_stageClearQuitButton != null)
        {
            _stageClearQuitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }

        if (_gameOverRestartButton != null)
        {
            _gameOverRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (_gameOverQuitButton != null)
        {
            _gameOverQuitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }

        if (_viewModel != null)
        {
            _viewModel.HeroHealthChanged -= OnHeroHealthChanged;
            _viewModel.RemainingEnemyCountChanged -= OnRemainingEnemyCountChanged;
            _viewModel.StageCleared -= OnStageCleared;
            _viewModel.StageFailed -= OnStageFailed;
        }

        _viewModel = null;
    }

    private void OnRestartButtonClicked()
    {
        _viewModel?.RequestRestart();
    }

    private void OnQuitButtonClicked()
    {
        _viewModel?.RequestQuit();
    }

    private void OnHeroHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHeroHealth(currentHealth, maxHealth);
    }

    private void OnRemainingEnemyCountChanged(int remainingEnemyCount)
    {
        UpdateRemainingEnemyCount(remainingEnemyCount);
    }

    private void OnStageCleared()
    {
        _stageClearPanel.SetActive(true);
    }

    private void OnStageFailed()
    {
        _gameOverPanel.SetActive(true);
    }

    private void UpdateHeroHealth(float currentHealth, float maxHealth)
    {
        float clampedMaxHealth = Mathf.Max(0f, maxHealth);
        float clampedCurrentHealth = Mathf.Clamp(currentHealth, 0f, clampedMaxHealth);

        _heroHpSlider.minValue = 0f;
        _heroHpSlider.maxValue = Mathf.Max(1f, clampedMaxHealth);
        _heroHpSlider.value = clampedCurrentHealth;
        _heroHpText.text = $"{clampedCurrentHealth:F0} / {clampedMaxHealth:F0}";
    }

    private void UpdateRemainingEnemyCount(int remainingEnemyCount)
    {
        _remainingEnemyText.text = $"남은 적: {Mathf.Max(0, remainingEnemyCount)}";
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
