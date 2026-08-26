using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BAUnitView))]
[RequireComponent(typeof(BAUnitCombatController))]
public class BAHeroVisualController : MonoBehaviour
{
    private static readonly int _isMovingParameterHash = Animator.StringToHash("IsMoving");
    private static readonly int _attackParameterHash = Animator.StringToHash("Attack");

    [SerializeField] private GameObject _normalVisual;
    [SerializeField] private GameObject _assembledVisual;
    [SerializeField] private float _movementThreshold = 0.001f;
    [SerializeField] private float _movementStopDelay = 0.08f;

    private BAUnitCombatController _combatController;
    private Animator _animator;
    private Vector3 _previousPosition;
    private float _lastMovementTime;
    private bool _isInitialized;
    private bool _isAssembled;
    private bool _isMoving;

    public bool IsInitialized => _isInitialized;
    public bool IsAssembled => _isAssembled;

    private void Awake()
    {
        _combatController = GetComponent<BAUnitCombatController>();
        _combatController.AttackPerformed += OnAttackPerformed;

        if (_normalVisual == null || _assembledVisual == null)
        {
            Debug.LogError("용자 로봇의 일반 및 합체 Visual 참조가 설정되지 않았습니다.");
            return;
        }

        if (_normalVisual == _assembledVisual)
        {
            Debug.LogError("일반 Visual과 합체 Visual에 같은 오브젝트가 설정되어 있습니다.");
            return;
        }

        _animator = _normalVisual.GetComponent<Animator>();

        if (_animator == null)
        {
            Debug.LogError("용자 로봇의 일반 Visual에서 Animator를 찾을 수 없습니다.");
            return;
        }

        _isMoving = _animator.GetBool(_isMovingParameterHash);
        SetMoving(false);
        _previousPosition = transform.position;
        _lastMovementTime = Time.time;
        _isInitialized = true;
        TrySetAssembled(false);
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

    public bool TrySetAssembled(bool isAssembled)
    {
        if (!_isInitialized)
        {
            return false;
        }

        if (isAssembled)
        {
            _animator.ResetTrigger(_attackParameterHash);
        }

        _normalVisual.SetActive(!isAssembled);
        _assembledVisual.SetActive(isAssembled);
        _isAssembled = isAssembled;
        return true;
    }

    private void OnDestroy()
    {
        if (_combatController != null)
        {
            _combatController.AttackPerformed -= OnAttackPerformed;
        }
    }

    private void OnAttackPerformed()
    {
        if (!_isInitialized ||
            _isAssembled ||
            !_normalVisual.activeInHierarchy ||
            !_animator.isActiveAndEnabled)
        {
            return;
        }

        _animator.SetTrigger(_attackParameterHash);
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
