using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BAUnitView))]
[RequireComponent(typeof(Rigidbody2D))]
public class BAUnitCombatController : MonoBehaviour, IBAPoolable
{
    private const float _initializationTimeout = 10f;
    private const float _targetSearchInterval = 0.2f;

    [SerializeField] private string _unitId;
    [SerializeField] private bool _usesAttackAnimationEvent;

    private BAUnitView _unitView;
    private Rigidbody2D _rigidbody;
    private BAUnitView _target;
    private BAUnitView _pendingAttackTarget;
    private bool _isInitialized;
    private float _nextTargetSearchTime;
    private float _nextAttackTime;

    public bool HasPendingAttack => _pendingAttackTarget != null;

    public event Action AttackPerformed;

    private void Awake()
    {
        _unitView = GetComponent<BAUnitView>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private IEnumerator Start()
    {
        float elapsedTime = 0f;

        while ((BAGameManager.Instance == null || !BAGameManager.Instance.IsInitialized) &&
               elapsedTime < _initializationTimeout)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (BAGameManager.Instance == null || !BAGameManager.Instance.IsInitialized)
        {
            Debug.LogError("게임 매니저 초기화 대기 시간이 초과되어 유닛 초기화를 중단합니다.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(_unitId))
        {
            Debug.LogError("유닛 ID가 설정되지 않아 유닛 초기화를 중단합니다.");
            yield break;
        }

        if (BABattleManager.Instance == null ||
            !BABattleManager.Instance.TryBindUnit(_unitId, _unitView))
        {
            Debug.LogError($"유닛 바인딩에 실패했습니다: {_unitId}");
            yield break;
        }

        _isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!_isInitialized || _unitView.IsDead)
        {
            StopHorizontalMovement();
            return;
        }

        if (_target == null || !_target.gameObject.activeInHierarchy || _target.IsDead)
        {
            _target = null;
        }

        if (_target != null)
        {
            Vector2 targetOffset = (Vector2)_target.transform.position - _rigidbody.position;
            float detectionRangeSquared = _unitView.DetectionRange * _unitView.DetectionRange;

            if (targetOffset.sqrMagnitude > detectionRangeSquared)
            {
                _target = null;
            }
        }

        if (_pendingAttackTarget != null)
        {
            StopHorizontalMovement();
            return;
        }

        if (_target == null && Time.time >= _nextTargetSearchTime)
        {
            _nextTargetSearchTime = Time.time + _targetSearchInterval;

            if (BABattleManager.Instance != null)
            {
                BABattleManager.Instance.TryFindNearestEnemy(_unitView, out _target);
            }
        }

        if (_target == null)
        {
            StopHorizontalMovement();
            return;
        }

        Vector2 offset = (Vector2)_target.transform.position - _rigidbody.position;
        float attackRangeSquared = _unitView.AttackRange * _unitView.AttackRange;

        if (offset.sqrMagnitude > attackRangeSquared)
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = Mathf.Sign(offset.x) * _unitView.MoveSpeed;
            _rigidbody.linearVelocity = velocity;
            return;
        }

        StopHorizontalMovement();

        if (_unitView.AttackDamage <= 0f || Time.time < _nextAttackTime)
        {
            return;
        }

        if (BABattleManager.Instance == null)
        {
            return;
        }

        if (_usesAttackAnimationEvent)
        {
            _pendingAttackTarget = _target;
            _nextAttackTime = Time.time + Mathf.Max(0f, _unitView.AttackInterval);
            AttackPerformed?.Invoke();
            return;
        }

        if (!BABattleManager.Instance.TryApplyDamage(
                _unitView,
                _target,
                _unitView.AttackDamage))
        {
            return;
        }

        _nextAttackTime = Time.time + Mathf.Max(0f, _unitView.AttackInterval);
        AttackPerformed?.Invoke();
    }

    public void ApplyPendingAttackDamage()
    {
        BAUnitView pendingTarget = _pendingAttackTarget;
        _pendingAttackTarget = null;

        if (!_isInitialized || _unitView == null || _unitView.IsDead)
        {
            return;
        }

        if (pendingTarget == null ||
            !pendingTarget.gameObject.activeInHierarchy ||
            pendingTarget.IsDead)
        {
            return;
        }

        if (BABattleManager.Instance == null)
        {
            return;
        }

        BABattleManager.Instance.TryApplyDamage(
            _unitView,
            pendingTarget,
            _unitView.AttackDamage);
    }

    public void OnSpawned()
    {
        ResetCombatState();

        if (!_isInitialized)
        {
            return;
        }

        _unitView.ResetState();
    }

    public void OnDespawned()
    {
        ResetCombatState();
    }

    private void OnDisable()
    {
        ResetCombatState();
    }

    private void OnDestroy()
    {
        if (BABattleManager.Instance != null)
        {
            BABattleManager.Instance.ReleaseUnit(_unitView);
        }
    }

    private void ResetCombatState()
    {
        _target = null;
        _pendingAttackTarget = null;
        _nextTargetSearchTime = 0f;
        _nextAttackTime = 0f;

        if (_rigidbody == null)
        {
            return;
        }

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
    }

    private void StopHorizontalMovement()
    {
        if (_rigidbody == null)
        {
            return;
        }

        Vector2 velocity = _rigidbody.linearVelocity;
        velocity.x = 0f;
        _rigidbody.linearVelocity = velocity;
    }
}
