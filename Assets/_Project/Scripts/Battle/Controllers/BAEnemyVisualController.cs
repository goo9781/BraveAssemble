using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BAUnitView))]
public class BAEnemyVisualController : MonoBehaviour, IBAPoolable
{
    private static readonly int _isMovingParameterHash = Animator.StringToHash("IsMoving");

    [SerializeField] private Animator _animator;
    [SerializeField] private float _movementThreshold = 0.001f;
    [SerializeField] private float _movementStopDelay = 0.08f;

    private Vector3 _previousPosition;
    private float _lastMovementTime;
    private bool _isInitialized;
    private bool _isMoving;

    private void Awake()
    {
        if (_animator == null)
        {
            Debug.LogError("적 Visual의 Animator 참조가 설정되지 않았습니다.");
            return;
        }

        _isInitialized = true;
        ResetMovementState();
    }

    private void LateUpdate()
    {
        if (!_isInitialized)
        {
            return;
        }

        Vector3 currentPosition = transform.position;
        float movementThreshold = Mathf.Max(0f, _movementThreshold);
        bool hasMoved =
            (currentPosition - _previousPosition).sqrMagnitude >
            movementThreshold * movementThreshold;

        _previousPosition = currentPosition;

        if (hasMoved)
        {
            _lastMovementTime = Time.time;
            SetMoving(true);
            return;
        }

        if (_isMoving &&
            Time.time - _lastMovementTime >= Mathf.Max(0f, _movementStopDelay))
        {
            SetMoving(false);
        }
    }

    public void OnSpawned()
    {
        if (!_isInitialized)
        {
            return;
        }

        ResetMovementState();
    }

    public void OnDespawned()
    {
        if (!_isInitialized)
        {
            return;
        }

        SetMoving(false);
    }

    private void ResetMovementState()
    {
        _previousPosition = transform.position;
        _lastMovementTime = Time.time;
        _isMoving = false;
        _animator.SetBool(_isMovingParameterHash, false);
    }

    private void SetMoving(bool isMoving)
    {
        if (_isMoving == isMoving)
        {
            return;
        }

        _isMoving = isMoving;
        _animator.SetBool(_isMovingParameterHash, _isMoving);
    }
}
