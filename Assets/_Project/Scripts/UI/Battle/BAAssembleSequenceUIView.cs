using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BAAssembleSequenceUIView : MonoBehaviour
{
    private static readonly int _sequenceStateHash = Animator.StringToHash("BAAssembleSequence");

    [SerializeField] private Animator _animator;
    [SerializeField] private CanvasGroup _canvasGroup;

    private bool _isPlaying;
    private bool _isApplyEventInvoked;
    private bool _isCompleteEventInvoked;

    public bool IsPlaying => _isPlaying;

    public event Action ApplyAssembleRequested;
    public event Action SequenceCompleted;

    public bool Play()
    {
        if (_isPlaying || _animator == null || _canvasGroup == null)
        {
            return false;
        }

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
        ResetExecutionState();
    }

    private void OnDestroy()
    {
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

    private bool IsAnimatorActive()
    {
        return
            _animator != null &&
            _animator.isActiveAndEnabled &&
            _animator.gameObject.activeInHierarchy;
    }
}
