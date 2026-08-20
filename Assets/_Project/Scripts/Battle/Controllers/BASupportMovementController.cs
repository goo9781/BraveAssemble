using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BASupportMovementController : MonoBehaviour, IBAPoolable
{
    private enum MovementState
    {
        None = 0,
        MovingToAction,
        MovingToExit
    }

    private MovementState _movementState;
    private Vector3 _actionPosition;
    private Vector3 _exitPosition;
    private float _moveSpeed;
    private Action _onActionReached;
    private Action _onMovementCompleted;

    public bool IsMoving => _movementState != MovementState.None;

    public bool TryStartMovement(
        Vector3 actionPosition,
        Vector3 exitPosition,
        float moveSpeed,
        Action onActionReached,
        Action onMovementCompleted)
    {
        if (IsMoving || moveSpeed <= 0f)
        {
            return false;
        }

        _actionPosition = actionPosition;
        _exitPosition = exitPosition;
        _moveSpeed = moveSpeed;
        _onActionReached = onActionReached;
        _onMovementCompleted = onMovementCompleted;
        _movementState = MovementState.MovingToAction;
        return true;
    }

    private void Update()
    {
        if (_movementState == MovementState.MovingToAction)
        {
            MoveToActionPosition();
        }
        else if (_movementState == MovementState.MovingToExit)
        {
            MoveToExitPosition();
        }
    }

    public void OnSpawned()
    {
        ResetMovementState();
    }

    public void OnDespawned()
    {
        ResetMovementState();
    }

    private void MoveToActionPosition()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            _actionPosition,
            _moveSpeed * Time.deltaTime);

        if ((transform.position - _actionPosition).sqrMagnitude > Mathf.Epsilon)
        {
            return;
        }

        transform.position = _actionPosition;
        Action onActionReached = _onActionReached;
        _onActionReached = null;
        onActionReached?.Invoke();

        if (_movementState == MovementState.MovingToAction)
        {
            _movementState = MovementState.MovingToExit;
        }
    }

    private void MoveToExitPosition()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            _exitPosition,
            _moveSpeed * Time.deltaTime);

        if ((transform.position - _exitPosition).sqrMagnitude > Mathf.Epsilon)
        {
            return;
        }

        transform.position = _exitPosition;
        Action onMovementCompleted = _onMovementCompleted;
        ResetMovementState();
        onMovementCompleted?.Invoke();
    }

    private void ResetMovementState()
    {
        _movementState = MovementState.None;
        _actionPosition = Vector3.zero;
        _exitPosition = Vector3.zero;
        _moveSpeed = 0f;
        _onActionReached = null;
        _onMovementCompleted = null;
    }
}
