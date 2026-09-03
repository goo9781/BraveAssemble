using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BAAssembleSequenceUIView : MonoBehaviour
{
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
        _animator.Rebind();
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
        ResetExecutionState();
        ResetAnimator();

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ResetExecutionState();
        ResetAnimator();
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

    private void ResetAnimator()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.Rebind();
        _animator.Update(0f);
    }
}
