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
    [SerializeField] private Button _skillButton;
    [SerializeField] private TMP_Text _skillNameText;
    [SerializeField] private TMP_Text _skillCooldownText;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _pauseRestartButton;
    [SerializeField] private Button _pauseQuitButton;
    [SerializeField] private Slider _assembleGaugeSlider;
    [SerializeField] private Button _assembleButton;
    [SerializeField] private TMP_Text _assembleNameText;
    [SerializeField] private TMP_Text _assembleDurationText;
    [SerializeField] private Button _supportButton;
    [SerializeField] private TMP_Text _supportNameText;
    [SerializeField] private TMP_Text _supportCooldownText;

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
            _gameOverQuitButton == null ||
            _skillButton == null ||
            _skillNameText == null ||
            _skillCooldownText == null ||
            _pauseButton == null ||
            _pausePanel == null ||
            _resumeButton == null ||
            _pauseRestartButton == null ||
            _pauseQuitButton == null ||
            _assembleGaugeSlider == null ||
            _assembleButton == null ||
            _assembleNameText == null ||
            _assembleDurationText == null ||
            _supportButton == null ||
            _supportNameText == null ||
            _supportCooldownText == null)
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
        _viewModel.SkillCooldownChanged += OnSkillCooldownChanged;
        _viewModel.PauseStateChanged += OnPauseStateChanged;
        _viewModel.AssembleGaugeChanged += OnAssembleGaugeChanged;
        _viewModel.AssembleDurationChanged += OnAssembleDurationChanged;
        _viewModel.AssembleStateChanged += OnAssembleStateChanged;
        _viewModel.SupportCooldownChanged += OnSupportCooldownChanged;
        _viewModel.SupportActiveStateChanged += OnSupportActiveStateChanged;

        _stageClearRestartButton.onClick.AddListener(OnRestartButtonClicked);
        _stageClearQuitButton.onClick.AddListener(OnQuitButtonClicked);
        _gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
        _gameOverQuitButton.onClick.AddListener(OnQuitButtonClicked);
        _skillButton.onClick.AddListener(OnSkillButtonClicked);
        _pauseButton.onClick.AddListener(OnPauseButtonClicked);
        _resumeButton.onClick.AddListener(OnResumeButtonClicked);
        _pauseRestartButton.onClick.AddListener(OnRestartButtonClicked);
        _pauseQuitButton.onClick.AddListener(OnQuitButtonClicked);
        _assembleButton.onClick.AddListener(OnAssembleButtonClicked);
        _supportButton.onClick.AddListener(OnSupportButtonClicked);

        UpdateHeroHealth(_viewModel.HeroCurrentHealth, _viewModel.HeroMaxHealth);
        UpdateRemainingEnemyCount(_viewModel.RemainingEnemyCount);
        _stageClearPanel.SetActive(_viewModel.IsStageCleared);
        _gameOverPanel.SetActive(_viewModel.IsStageFailed);
        _skillNameText.text = _viewModel.SkillDisplayName;
        UpdateSkillCooldown(
            _viewModel.SkillRemainingCooldown,
            _viewModel.SkillCooldown);
        _assembleNameText.text = _viewModel.AssembleDisplayName;
        UpdateAssembleGauge(
            _viewModel.AssembleCurrentGauge,
            _viewModel.AssembleMaxGauge);
        UpdateAssembleDuration(
            _viewModel.AssembleRemainingDuration,
            _viewModel.AssembleDuration);
        UpdateAssembleButtonState();
        _supportNameText.text = _viewModel.SupportDisplayName;
        UpdateSupportCooldown(
            _viewModel.SupportRemainingCooldown,
            _viewModel.SupportCooldown);
        UpdateSupportButtonState();
        UpdatePauseState(_viewModel.IsPaused);

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

        if (_skillButton != null)
        {
            _skillButton.onClick.RemoveListener(OnSkillButtonClicked);
        }

        if (_pauseButton != null)
        {
            _pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
        }

        if (_resumeButton != null)
        {
            _resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
        }

        if (_pauseRestartButton != null)
        {
            _pauseRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (_pauseQuitButton != null)
        {
            _pauseQuitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }

        if (_assembleButton != null)
        {
            _assembleButton.onClick.RemoveListener(OnAssembleButtonClicked);
        }

        if (_supportButton != null)
        {
            _supportButton.onClick.RemoveListener(OnSupportButtonClicked);
        }

        if (_viewModel != null)
        {
            _viewModel.HeroHealthChanged -= OnHeroHealthChanged;
            _viewModel.RemainingEnemyCountChanged -= OnRemainingEnemyCountChanged;
            _viewModel.StageCleared -= OnStageCleared;
            _viewModel.StageFailed -= OnStageFailed;
            _viewModel.SkillCooldownChanged -= OnSkillCooldownChanged;
            _viewModel.PauseStateChanged -= OnPauseStateChanged;
            _viewModel.AssembleGaugeChanged -= OnAssembleGaugeChanged;
            _viewModel.AssembleDurationChanged -= OnAssembleDurationChanged;
            _viewModel.AssembleStateChanged -= OnAssembleStateChanged;
            _viewModel.SupportCooldownChanged -= OnSupportCooldownChanged;
            _viewModel.SupportActiveStateChanged -= OnSupportActiveStateChanged;
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

    private void OnSkillButtonClicked()
    {
        _viewModel?.RequestUseSkill();
    }

    private void OnPauseButtonClicked()
    {
        _viewModel?.RequestPause();
    }

    private void OnResumeButtonClicked()
    {
        _viewModel?.RequestResume();
    }

    private void OnAssembleButtonClicked()
    {
        _viewModel?.RequestAssemble();
    }

    private void OnSupportButtonClicked()
    {
        _viewModel?.RequestUseSupport();
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
        _skillButton.interactable = false;
        _pausePanel.SetActive(false);
        _pauseButton.interactable = false;
        _assembleButton.interactable = false;
        _assembleButton.gameObject.SetActive(false);
        _supportButton.interactable = false;
    }

    private void OnStageFailed()
    {
        _gameOverPanel.SetActive(true);
        _skillButton.interactable = false;
        _pausePanel.SetActive(false);
        _pauseButton.interactable = false;
        _assembleButton.interactable = false;
        _assembleButton.gameObject.SetActive(false);
        _supportButton.interactable = false;
    }

    private void OnSkillCooldownChanged(float remainingCooldown, float cooldown)
    {
        UpdateSkillCooldown(remainingCooldown, cooldown);
    }

    private void OnPauseStateChanged(bool isPaused)
    {
        UpdatePauseState(isPaused);
    }

    private void OnAssembleGaugeChanged(float currentGauge, float maxGauge)
    {
        UpdateAssembleGauge(currentGauge, maxGauge);
    }

    private void OnAssembleDurationChanged(float remainingDuration, float duration)
    {
        UpdateAssembleDuration(remainingDuration, duration);
    }

    private void OnAssembleStateChanged(bool isAssembled)
    {
        if (isAssembled)
        {
            UpdateAssembleDuration(
                _viewModel.AssembleRemainingDuration,
                _viewModel.AssembleDuration);
        }
        else
        {
            _assembleDurationText.text = string.Empty;
            UpdateAssembleGauge(
                _viewModel.AssembleCurrentGauge,
                _viewModel.AssembleMaxGauge);
        }

        UpdateAssembleButtonState();
        UpdateSupportButtonState();
    }

    private void OnSupportCooldownChanged(float remainingCooldown, float cooldown)
    {
        UpdateSupportCooldown(remainingCooldown, cooldown);
    }

    private void OnSupportActiveStateChanged(bool isSupportActive)
    {
        UpdateSupportButtonState();
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

    private void UpdateSkillCooldown(float remainingCooldown, float cooldown)
    {
        float clampedRemainingCooldown = Mathf.Clamp(
            remainingCooldown,
            0f,
            Mathf.Max(0f, cooldown));

        if (clampedRemainingCooldown > 0f)
        {
            _skillCooldownText.text = $"{clampedRemainingCooldown:F1}";
        }
        else
        {
            _skillCooldownText.text = string.Empty;
        }

        _skillButton.interactable = _viewModel != null && _viewModel.CanUseSkill;
    }

    private void UpdatePauseState(bool isPaused)
    {
        _pausePanel.SetActive(isPaused);

        _skillButton.interactable =
            !isPaused &&
            _viewModel != null &&
            _viewModel.CanUseSkill;

        _pauseButton.interactable =
            !isPaused &&
            _viewModel != null &&
            !_viewModel.IsStageCleared &&
            !_viewModel.IsStageFailed;

        UpdateAssembleButtonState();
        UpdateSupportButtonState();
    }

    private void UpdateAssembleGauge(float currentGauge, float maxGauge)
    {
        if (_viewModel != null && _viewModel.IsAssembled)
        {
            UpdateAssembleButtonState();
            return;
        }

        float clampedMaxGauge = Mathf.Max(0f, maxGauge);

        _assembleGaugeSlider.minValue = 0f;
        _assembleGaugeSlider.maxValue = Mathf.Max(1f, clampedMaxGauge);
        _assembleGaugeSlider.value = Mathf.Clamp(currentGauge, 0f, clampedMaxGauge);
        UpdateAssembleButtonState();
    }

    private void UpdateAssembleDuration(float remainingDuration, float duration)
    {
        if (_viewModel != null && _viewModel.IsAssembled)
        {
            float clampedDuration = Mathf.Max(0f, duration);
            float clampedRemainingDuration = Mathf.Clamp(
                remainingDuration,
                0f,
                clampedDuration);

            _assembleGaugeSlider.minValue = 0f;
            _assembleGaugeSlider.maxValue = Mathf.Max(1f, clampedDuration);
            _assembleGaugeSlider.value = clampedRemainingDuration;

            if (clampedRemainingDuration > 0f)
            {
                _assembleDurationText.text = $"합체 {clampedRemainingDuration:F1}초";
            }
            else
            {
                _assembleDurationText.text = string.Empty;
            }
        }
        else
        {
            _assembleDurationText.text = string.Empty;
        }
    }

    private void UpdateAssembleButtonState()
    {
        bool canAssemble =
            _viewModel != null &&
            !_viewModel.IsPaused &&
            !_viewModel.IsStageCleared &&
            !_viewModel.IsStageFailed &&
            _viewModel.CanAssemble;

        _assembleButton.gameObject.SetActive(canAssemble);
        _assembleButton.interactable = canAssemble;
    }

    private void UpdateSupportCooldown(float remainingCooldown, float cooldown)
    {
        float clampedRemainingCooldown = Mathf.Clamp(
            remainingCooldown,
            0f,
            Mathf.Max(0f, cooldown));

        if (clampedRemainingCooldown > 0f)
        {
            _supportCooldownText.text = $"{clampedRemainingCooldown:F1}";
        }
        else
        {
            _supportCooldownText.text = string.Empty;
        }

        UpdateSupportButtonState();
    }

    private void UpdateSupportButtonState()
    {
        _supportButton.interactable =
            _viewModel != null &&
            !_viewModel.IsPaused &&
            !_viewModel.IsStageCleared &&
            !_viewModel.IsStageFailed &&
            _viewModel.CanUseSupport;
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
