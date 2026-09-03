using DG.Tweening;
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
    [SerializeField] private TMP_Text _stageClearResultText;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TMP_Text _gameOverResultText;
    [SerializeField] private Button _stageClearRestartButton;
    [SerializeField] private Button _stageClearMainButton;
    [SerializeField] private Button _stageClearQuitButton;
    [SerializeField] private Button _gameOverRestartButton;
    [SerializeField] private Button _gameOverMainButton;
    [SerializeField] private Button _gameOverQuitButton;
    [SerializeField] private Button _skillButton;
    [SerializeField] private TMP_Text _skillNameText;
    [SerializeField] private TMP_Text _skillCooldownText;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _pauseRestartButton;
    [SerializeField] private Button _pauseMainButton;
    [SerializeField] private Button _pauseQuitButton;
    [SerializeField] private Slider _assembleGaugeSlider;
    [SerializeField] private GameObject _assembleReadyPanel;
    [SerializeField] private RectTransform _assembleReadyBackground;
    [SerializeField] private CanvasGroup _assembleReadyCanvasGroup;
    [SerializeField] private Button _assembleButton;
    [SerializeField] private TMP_Text _assembleNameText;
    [SerializeField] private TMP_Text _assembleDurationText;
    [SerializeField] private BAAssembleSequenceUIView _assembleSequenceView;
    [SerializeField] private Button _supportButton;
    [SerializeField] private TMP_Text _supportNameText;
    [SerializeField] private TMP_Text _supportCooldownText;

    private BABattleHudViewModel _viewModel;
    private Sequence _assembleReadySequence;
    private Tween _assembleButtonLoopTween;
    private bool _wasAssembleReady;
    private bool _isAssembleSequencePlaying;
    private bool _isAssembleApplied;

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
            _stageClearResultText == null ||
            _gameOverPanel == null ||
            _gameOverResultText == null ||
            _stageClearRestartButton == null ||
            _stageClearMainButton == null ||
            _stageClearQuitButton == null ||
            _gameOverRestartButton == null ||
            _gameOverMainButton == null ||
            _gameOverQuitButton == null ||
            _skillButton == null ||
            _skillNameText == null ||
            _skillCooldownText == null ||
            _pauseButton == null ||
            _pausePanel == null ||
            _resumeButton == null ||
            _pauseRestartButton == null ||
            _pauseMainButton == null ||
            _pauseQuitButton == null ||
            _assembleGaugeSlider == null ||
            _assembleReadyPanel == null ||
            _assembleReadyBackground == null ||
            _assembleReadyCanvasGroup == null ||
            _assembleButton == null ||
            _assembleNameText == null ||
            _assembleDurationText == null ||
            _assembleSequenceView == null ||
            _supportButton == null ||
            _supportNameText == null ||
            _supportCooldownText == null)
        {
            Debug.LogError("전투 HUD의 Inspector 참조가 모두 설정되지 않았습니다.");
            return false;
        }

        Unbind();

        _viewModel = viewModel;
        ResetAssembleReadyState();
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
        _assembleSequenceView.ApplyAssembleRequested += OnApplyAssembleRequested;
        _assembleSequenceView.SequenceCompleted += OnAssembleSequenceCompleted;

        _stageClearRestartButton.onClick.AddListener(OnRestartButtonClicked);
        _stageClearMainButton.onClick.AddListener(OnMainButtonClicked);
        _stageClearQuitButton.onClick.AddListener(OnQuitButtonClicked);
        _gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
        _gameOverMainButton.onClick.AddListener(OnMainButtonClicked);
        _gameOverQuitButton.onClick.AddListener(OnQuitButtonClicked);
        _skillButton.onClick.AddListener(OnSkillButtonClicked);
        _pauseButton.onClick.AddListener(OnPauseButtonClicked);
        _resumeButton.onClick.AddListener(OnResumeButtonClicked);
        _pauseRestartButton.onClick.AddListener(OnRestartButtonClicked);
        _pauseMainButton.onClick.AddListener(OnMainButtonClicked);
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

    public void SetStageClearResult(string resultText)
    {
        _stageClearResultText.text = resultText ?? string.Empty;
    }

    public void SetGameOverResult(string resultText)
    {
        _gameOverResultText.text = resultText ?? string.Empty;
    }

    public void Unbind()
    {
        ResetAssembleSequenceState();
        ResetAssembleReadyState();

        if (_stageClearRestartButton != null)
        {
            _stageClearRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (_stageClearMainButton != null)
        {
            _stageClearMainButton.onClick.RemoveListener(OnMainButtonClicked);
        }

        if (_stageClearQuitButton != null)
        {
            _stageClearQuitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }

        if (_gameOverRestartButton != null)
        {
            _gameOverRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (_gameOverMainButton != null)
        {
            _gameOverMainButton.onClick.RemoveListener(OnMainButtonClicked);
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

        if (_pauseMainButton != null)
        {
            _pauseMainButton.onClick.RemoveListener(OnMainButtonClicked);
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

        if (_assembleSequenceView != null)
        {
            _assembleSequenceView.ApplyAssembleRequested -= OnApplyAssembleRequested;
            _assembleSequenceView.SequenceCompleted -= OnAssembleSequenceCompleted;
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

    private void OnMainButtonClicked()
    {
        _viewModel?.RequestMain();
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
        if (!IsAssembleReady() ||
            _isAssembleSequencePlaying ||
            _assembleSequenceView == null ||
            _assembleSequenceView.IsPlaying)
        {
            return;
        }

        KillAssembleReadyTweens();
        _assembleButton.interactable = false;

        if (!_assembleSequenceView.Play())
        {
            ResetAssembleReadyState();
            UpdateAssembleButtonState();
            return;
        }

        _isAssembleSequencePlaying = true;
        _isAssembleApplied = false;
        _viewModel.RequestPause();
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
        ResetAssembleSequenceState();

        if (_viewModel != null)
        {
            SetStageClearResult(
                $"클리어 시간: {_viewModel.BattleElapsedTime:F1}초\n처치 적: {_viewModel.DefeatedEnemyCount}");
        }

        _stageClearPanel.SetActive(true);
        _skillButton.interactable = false;
        _pausePanel.SetActive(false);
        _pauseButton.interactable = false;
        _assembleButton.interactable = false;
        _assembleButton.gameObject.SetActive(false);
        ResetAssembleReadyState();
        _supportButton.interactable = false;
    }

    private void OnStageFailed()
    {
        ResetAssembleSequenceState();

        if (_viewModel != null)
        {
            SetGameOverResult(
                $"생존 시간: {_viewModel.BattleElapsedTime:F1}초\n처치 적: {_viewModel.DefeatedEnemyCount}");
        }

        _gameOverPanel.SetActive(true);
        _skillButton.interactable = false;
        _pausePanel.SetActive(false);
        _pauseButton.interactable = false;
        _assembleButton.interactable = false;
        _assembleButton.gameObject.SetActive(false);
        ResetAssembleReadyState();
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
            if (_isAssembleSequencePlaying)
            {
                _isAssembleApplied = true;
            }

            ResetAssembleReadyState();
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

    private void OnApplyAssembleRequested()
    {
        if (!_isAssembleSequencePlaying ||
            _isAssembleApplied ||
            _viewModel == null)
        {
            return;
        }

        _viewModel.RequestAssemble();
        _isAssembleApplied = _viewModel.IsAssembled;

        if (_isAssembleApplied)
        {
            return;
        }

        ResetAssembleSequenceState();

        if (!_viewModel.IsStageCleared && !_viewModel.IsStageFailed)
        {
            _viewModel.RequestResume();
        }

        ResetAssembleReadyState();
        UpdateAssembleButtonState();
    }

    private void OnAssembleSequenceCompleted()
    {
        if (!_isAssembleSequencePlaying)
        {
            return;
        }

        bool isAssembleApplied = _isAssembleApplied;
        ResetAssembleSequenceState();

        if (isAssembleApplied &&
            _viewModel != null &&
            !_viewModel.IsStageCleared &&
            !_viewModel.IsStageFailed)
        {
            _viewModel.RequestResume();
        }
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
        if (_isAssembleSequencePlaying)
        {
            _assembleButton.interactable = false;
            return;
        }

        bool isAssembleReady = IsAssembleReady();

        if (isAssembleReady && !_wasAssembleReady)
        {
            PlayAssembleReadyAnimation();
        }
        else if (!isAssembleReady && _wasAssembleReady)
        {
            HideAssembleReadyPanel();
        }

        _wasAssembleReady = isAssembleReady;
        _assembleButton.gameObject.SetActive(isAssembleReady);
        _assembleButton.interactable =
            isAssembleReady &&
            !_viewModel.IsPaused &&
            _assembleReadySequence == null;
    }

    private bool IsAssembleReady()
    {
        return
            _viewModel != null &&
            !_viewModel.IsAssembled &&
            !_viewModel.IsStageCleared &&
            !_viewModel.IsStageFailed &&
            _viewModel.CanAssemble;
    }

    private void PlayAssembleReadyAnimation()
    {
        KillAssembleReadyTweens();
        _assembleReadyPanel.SetActive(true);
        _assembleReadyCanvasGroup.alpha = 0f;
        _assembleReadyBackground.localScale = new Vector3(0f, 1f, 1f);
        _assembleButton.transform.localScale = Vector3.one * 0.8f;
        _assembleButton.gameObject.SetActive(true);
        _assembleButton.interactable = false;

        Sequence readySequence = DOTween.Sequence();
        _assembleReadySequence = readySequence;
        readySequence.Append(
            _assembleReadyBackground.DOScaleX(1f, 0.3f)
                .SetEase(Ease.OutQuad));
        readySequence.Join(
            _assembleReadyCanvasGroup.DOFade(1f, 0.3f)
                .SetEase(Ease.OutQuad));
        readySequence.Append(
            _assembleButton.transform.DOScale(1.08f, 0.18f)
                .SetEase(Ease.OutBack));
        readySequence.Append(
            _assembleButton.transform.DOScale(1f, 0.12f)
                .SetEase(Ease.OutQuad));
        readySequence.OnComplete(() =>
        {
            if (_assembleReadySequence != readySequence)
            {
                return;
            }

            _assembleReadySequence = null;
            OnAssembleReadyAnimationCompleted();
        });
        readySequence.OnKill(() =>
        {
            if (_assembleReadySequence == readySequence)
            {
                _assembleReadySequence = null;
            }
        });
    }

    private void OnAssembleReadyAnimationCompleted()
    {
        if (!IsAssembleReady())
        {
            ResetAssembleReadyState();
            return;
        }

        _assembleButton.interactable = !_viewModel.IsPaused;
        StartAssembleButtonLoopAnimation();
    }

    private void StartAssembleButtonLoopAnimation()
    {
        _assembleButtonLoopTween?.Kill();
        Tween buttonLoopTween = _assembleButton.transform
            .DOScale(1.04f, 0.6f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        _assembleButtonLoopTween = buttonLoopTween;
        buttonLoopTween.OnKill(() =>
        {
            if (_assembleButtonLoopTween == buttonLoopTween)
            {
                _assembleButtonLoopTween = null;
            }
        });
    }

    private void HideAssembleReadyPanel()
    {
        KillAssembleReadyTweens();

        if (_assembleReadyPanel != null)
        {
            _assembleReadyPanel.SetActive(false);
        }

        if (_assembleReadyBackground != null)
        {
            _assembleReadyBackground.localScale = Vector3.one;
        }

        if (_assembleReadyCanvasGroup != null)
        {
            _assembleReadyCanvasGroup.alpha = 1f;
        }

        if (_assembleButton != null)
        {
            _assembleButton.transform.localScale = Vector3.one;
            _assembleButton.interactable = false;
            _assembleButton.gameObject.SetActive(false);
        }
    }

    private void ResetAssembleReadyState()
    {
        _wasAssembleReady = false;
        HideAssembleReadyPanel();
    }

    private void KillAssembleReadyTweens()
    {
        Sequence readySequence = _assembleReadySequence;
        _assembleReadySequence = null;
        readySequence?.Kill();

        Tween buttonLoopTween = _assembleButtonLoopTween;
        _assembleButtonLoopTween = null;
        buttonLoopTween?.Kill();
    }

    private void ResetAssembleSequenceState()
    {
        if (_assembleSequenceView != null)
        {
            _assembleSequenceView.Cancel();
        }

        _isAssembleSequencePlaying = false;
        _isAssembleApplied = false;
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

    private void OnEnable()
    {
        if (_viewModel == null)
        {
            return;
        }

        ResetAssembleReadyState();
        UpdateAssembleButtonState();
    }

    private void OnDisable()
    {
        ResetAssembleSequenceState();
        ResetAssembleReadyState();
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
