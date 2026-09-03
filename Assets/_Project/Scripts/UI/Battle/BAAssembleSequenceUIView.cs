using System;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public class BAAssembleSequenceUIView : MonoBehaviour
{
    private const float _backgroundScaleMultiplier = 1.04f;
    private const float _backgroundScaleDuration = 3f;
    private const float _flashStartTime = 1.7f;
    private const float _flashFadeInDuration = 0.15f;
    private const float _flashFadeOutDuration = 0.25f;

    private static readonly int _sequenceStateHash = Animator.StringToHash("BAAssembleSequence");

    [SerializeField] private Animator _animator;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _backgroundRectTransform;
    [SerializeField] private CanvasGroup _combineFlashCanvasGroup;

    private Sequence _sequenceTween;
    private bool _isPlaying;
    private bool _isApplyEventInvoked;
    private bool _isCompleteEventInvoked;

    public bool IsPlaying => _isPlaying;

    public event Action ApplyAssembleRequested;
    public event Action SequenceCompleted;

    public bool Play()
    {
        if (_isPlaying ||
            _animator == null ||
            _canvasGroup == null ||
            _backgroundRectTransform == null ||
            _combineFlashCanvasGroup == null)
        {
            return false;
        }

        KillSequenceTween();
        ResetTweenVisualState();
        ResetExecutionState();
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;
        _animator.enabled = true;

        if (!IsAnimatorActive())
        {
            gameObject.SetActive(false);
            return false;
        }

        _animator.Rebind();
        _animator.Play(_sequenceStateHash, 0, 0f);
        _animator.Update(0f);
        _isPlaying = true;
        PlaySequenceTween();
        return true;
    }

    public void OnApplyAssembleAnimationEvent()
    {
        if (!_isPlaying || _isApplyEventInvoked)
        {
            return;
        }

        _isApplyEventInvoked = true;
        ApplyAssembleRequested?.Invoke();
    }

    public void OnSequenceCompletedAnimationEvent()
    {
        if (!_isPlaying || _isCompleteEventInvoked)
        {
            return;
        }

        _isCompleteEventInvoked = true;
        _isPlaying = false;
        SequenceCompleted?.Invoke();
    }

    public void Cancel()
    {
        KillSequenceTween();
        ResetTweenVisualState();

        if (gameObject.activeInHierarchy && IsAnimatorActive())
        {
            _animator.Rebind();
            _animator.Play(_sequenceStateHash, 0, 0f);
            _animator.Update(0f);
        }

        ResetExecutionState();
        ResetCanvasGroup();

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        KillSequenceTween();
        ResetTweenVisualState();
        ResetExecutionState();
    }

    private void OnDestroy()
    {
        KillSequenceTween();
        ResetTweenVisualState();
        ApplyAssembleRequested = null;
        SequenceCompleted = null;
    }

    private void ResetExecutionState()
    {
        _isPlaying = false;
        _isApplyEventInvoked = false;
        _isCompleteEventInvoked = false;
    }

    private void ResetCanvasGroup()
    {
        if (_canvasGroup == null)
        {
            return;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;
    }

    private void PlaySequenceTween()
    {
        Sequence sequenceTween = DOTween.Sequence();
        _sequenceTween = sequenceTween;
        sequenceTween.Append(
            _backgroundRectTransform
                .DOScale(Vector3.one * _backgroundScaleMultiplier, _backgroundScaleDuration)
                .SetEase(Ease.InOutSine));
        sequenceTween.Insert(
            _flashStartTime,
            _combineFlashCanvasGroup
                .DOFade(1f, _flashFadeInDuration)
                .SetEase(Ease.Linear));
        sequenceTween.Insert(
            _flashStartTime + _flashFadeInDuration,
            _combineFlashCanvasGroup
                .DOFade(0f, _flashFadeOutDuration)
                .SetEase(Ease.Linear));
        sequenceTween.SetUpdate(true);
        sequenceTween.OnComplete(() =>
        {
            if (_sequenceTween == sequenceTween)
            {
                _sequenceTween = null;
            }
        });
        sequenceTween.OnKill(() =>
        {
            if (_sequenceTween == sequenceTween)
            {
                _sequenceTween = null;
            }
        });
    }

    private void KillSequenceTween()
    {
        Sequence sequenceTween = _sequenceTween;
        _sequenceTween = null;
        sequenceTween?.Kill();
    }

    private void ResetTweenVisualState()
    {
        if (_backgroundRectTransform != null)
        {
            _backgroundRectTransform.localScale = Vector3.one;
        }

        if (_combineFlashCanvasGroup != null)
        {
            _combineFlashCanvasGroup.alpha = 0f;
        }
    }

    private bool IsAnimatorActive()
    {
        return
            _animator != null &&
            _animator.isActiveAndEnabled &&
            _animator.gameObject.activeInHierarchy;
    }
}
