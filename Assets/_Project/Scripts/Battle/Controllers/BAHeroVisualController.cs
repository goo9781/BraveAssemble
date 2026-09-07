using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BAUnitView))]
[RequireComponent(typeof(BAUnitCombatController))]
public class BAHeroVisualController : MonoBehaviour
{
    private static readonly int _isMovingParameterHash = Animator.StringToHash("IsMoving");
    private static readonly int _attackParameterHash = Animator.StringToHash("Attack");
    private static readonly int _skillParameterHash = Animator.StringToHash("Skill");
    private static readonly int _playParameterHash = Animator.StringToHash("Play");
    private static readonly int _normalSkillStateHash = Animator.StringToHash("BraveRobot_Skill");
    private static readonly int _normalSkillStateFullPathHash = Animator.StringToHash("Base Layer.BraveRobot_Skill");
    private static readonly int _assembledSkillStateHash = Animator.StringToHash("BraveRobot_Assembled_001_Skill");
    private static readonly int _assembledSkillStateFullPathHash = Animator.StringToHash("Base Layer.BraveRobot_Assembled_001_Skill");
    private static readonly int _normalIdleStateHash = Animator.StringToHash("BraveRobot_Idle");
    private static readonly int _assembledIdleStateHash = Animator.StringToHash("BraveRobot_Assembled_001_Idle");

    [SerializeField] private GameObject _normalVisual;
    [SerializeField] private GameObject _assembledVisual;
    [SerializeField] private Animator _normalSkillEffectAnimator;
    [SerializeField] private Animator _assembledSkillEffectAnimator;
    [SerializeField] private Animator _assembledSupportEffectAnimator;
    [SerializeField] private float _movementThreshold = 0.001f;
    [SerializeField] private float _movementStopDelay = 0.08f;

    private BAUnitCombatController _combatController;
    private BASkillManager _skillManager;
    private BASupportManager _supportManager;
    private Animator _normalAnimator;
    private Animator _assembledAnimator;
    private Vector3 _previousPosition;
    private float _lastMovementTime;
    private bool _isInitialized;
    private bool _isAssembled;
    private bool _isMoving;
    private bool _isSkillAnimationActive;
    private bool _hasEnteredSkillState;
    private bool _hasPendingAttackAnimation;

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

        _normalAnimator = _normalVisual.GetComponent<Animator>();
        _assembledAnimator = _assembledVisual.GetComponent<Animator>();

        if (_normalAnimator == null || _assembledAnimator == null)
        {
            Debug.LogError("용자 로봇의 일반 또는 합체 Visual에서 Animator를 찾을 수 없습니다.");
            return;
        }

        if (_normalAnimator.runtimeAnimatorController == null ||
            _assembledAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("용자 로봇의 일반 또는 합체 Animator Controller가 설정되지 않았습니다.");
            return;
        }

        _isMoving = _normalAnimator.GetBool(_isMovingParameterHash);
        SetMoving(false);
        _previousPosition = transform.position;
        _lastMovementTime = Time.time;
        _isInitialized = true;
        TrySetAssembled(false);
    }

    private void Start()
    {
        _skillManager = BASkillManager.Instance;

        if (_skillManager != null)
        {
            _skillManager.SkillUsed += OnSkillUsed;
        }

        _supportManager = BASupportManager.Instance;

        if (_supportManager != null)
        {
            _supportManager.AssembledSupportUsed += OnAssembledSupportUsed;
        }
    }

    private void LateUpdate()
    {
        if (!_isInitialized)
        {
            return;
        }

        UpdateSkillAnimationState();

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

        bool previousIsAssembled = _isAssembled;
        bool hasAssembleStateChanged = previousIsAssembled != isAssembled;
        Animator inactiveAnimator = previousIsAssembled ? _assembledAnimator : _normalAnimator;

        if (hasAssembleStateChanged && _isSkillAnimationActive)
        {
            ResetSkillAnimationState();
        }

        if (hasAssembleStateChanged)
        {
            ResetSkillEffectTriggers();
            ResetAssembledSupportEffectTrigger();
        }

        if (hasAssembleStateChanged &&
            inactiveAnimator.gameObject.activeInHierarchy &&
            inactiveAnimator.isActiveAndEnabled &&
            inactiveAnimator.runtimeAnimatorController != null)
        {
            inactiveAnimator.ResetTrigger(_attackParameterHash);

            if (HasSkillTrigger(inactiveAnimator))
            {
                inactiveAnimator.ResetTrigger(_skillParameterHash);
            }
        }

        _normalVisual.SetActive(!isAssembled);
        _assembledVisual.SetActive(isAssembled);
        _isAssembled = isAssembled;

        Animator activeAnimator = isAssembled ? _assembledAnimator : _normalAnimator;

        if (activeAnimator.gameObject.activeInHierarchy &&
            activeAnimator.isActiveAndEnabled &&
            activeAnimator.runtimeAnimatorController != null)
        {
            activeAnimator.SetBool(_isMovingParameterHash, _isMoving);

            if (hasAssembleStateChanged && _combatController.HasPendingAttack)
            {
                activeAnimator.SetTrigger(_attackParameterHash);
            }
        }

        return true;
    }

    private void OnDestroy()
    {
        ResetAssembledSupportEffectTrigger();
        ResetSkillEffectTriggers();
        ResetSkillAnimationState();

        if (_combatController != null)
        {
            _combatController.AttackPerformed -= OnAttackPerformed;
        }

        if (_skillManager != null)
        {
            _skillManager.SkillUsed -= OnSkillUsed;
        }

        if (_supportManager != null)
        {
            _supportManager.AssembledSupportUsed -= OnAssembledSupportUsed;
        }
    }

    private void OnAttackPerformed()
    {
        if (!_isInitialized)
        {
            return;
        }

        Animator activeAnimator = _isAssembled ? _assembledAnimator : _normalAnimator;

        if (_isSkillAnimationActive || IsSkillState(activeAnimator))
        {
            _hasPendingAttackAnimation = true;
            return;
        }

        if (!activeAnimator.gameObject.activeInHierarchy ||
            !activeAnimator.isActiveAndEnabled ||
            activeAnimator.runtimeAnimatorController == null)
        {
            return;
        }

        activeAnimator.SetTrigger(_attackParameterHash);
    }

    private void OnSkillUsed(int hitCount)
    {
        if (!_isInitialized)
        {
            return;
        }

        Animator activeAnimator = _isAssembled ? _assembledAnimator : _normalAnimator;

        if (!activeAnimator.gameObject.activeInHierarchy ||
            !activeAnimator.isActiveAndEnabled ||
            activeAnimator.runtimeAnimatorController == null ||
            !HasSkillAnimation(activeAnimator))
        {
            return;
        }

        _hasPendingAttackAnimation =
            _hasPendingAttackAnimation ||
            _combatController.HasPendingAttack;
        activeAnimator.ResetTrigger(_attackParameterHash);
        _isSkillAnimationActive = true;
        _hasEnteredSkillState = false;
        _combatController.SetMovementPaused(true);
        SetMoving(false);
        activeAnimator.SetTrigger(_skillParameterHash);
        Animator skillEffectAnimator =
            _isAssembled ?
            _assembledSkillEffectAnimator :
            _normalSkillEffectAnimator;
        PlaySkillEffect(skillEffectAnimator);
    }

    private void OnAssembledSupportUsed()
    {
        if (!_isInitialized || !_isAssembled)
        {
            return;
        }

        PlaySkillEffect(_assembledSupportEffectAnimator);
    }

    private void UpdateSkillAnimationState()
    {
        if (!_isSkillAnimationActive)
        {
            return;
        }

        Animator activeAnimator = _isAssembled ? _assembledAnimator : _normalAnimator;

        if (!activeAnimator.gameObject.activeInHierarchy ||
            !activeAnimator.isActiveAndEnabled ||
            activeAnimator.runtimeAnimatorController == null)
        {
            ResetSkillAnimationState();
            return;
        }

        if (IsSkillState(activeAnimator))
        {
            _hasEnteredSkillState = true;
            return;
        }

        AnimatorStateInfo currentState = activeAnimator.GetCurrentAnimatorStateInfo(0);
        int idleStateHash =
            activeAnimator == _assembledAnimator ?
            _assembledIdleStateHash :
            _normalIdleStateHash;

        if (!_hasEnteredSkillState ||
            activeAnimator.IsInTransition(0) ||
            currentState.shortNameHash != idleStateHash)
        {
            return;
        }

        bool shouldPlayPendingAttack = _hasPendingAttackAnimation;
        _isSkillAnimationActive = false;
        _hasEnteredSkillState = false;
        _hasPendingAttackAnimation = false;
        _combatController.SetMovementPaused(false);
        _previousPosition = transform.position;
        _lastMovementTime = Time.time;

        if (shouldPlayPendingAttack)
        {
            activeAnimator.ResetTrigger(_attackParameterHash);
            activeAnimator.SetTrigger(_attackParameterHash);
        }
    }

    private bool IsSkillState(Animator animator)
    {
        if (animator == null ||
            !animator.gameObject.activeInHierarchy ||
            !animator.isActiveAndEnabled ||
            animator.runtimeAnimatorController == null)
        {
            return false;
        }

        int skillStateHash =
            animator == _assembledAnimator ?
            _assembledSkillStateHash :
            _normalSkillStateHash;
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.shortNameHash == skillStateHash)
        {
            return true;
        }

        return
            animator.IsInTransition(0) &&
            animator.GetNextAnimatorStateInfo(0).shortNameHash == skillStateHash;
    }

    private void ResetSkillAnimationState()
    {
        _isSkillAnimationActive = false;
        _hasEnteredSkillState = false;
        _hasPendingAttackAnimation = false;

        if (_combatController != null)
        {
            _combatController.SetMovementPaused(false);
        }
    }

    private bool HasSkillTrigger(Animator animator)
    {
        AnimatorControllerParameter[] parameters = animator.parameters;

        for (int index = 0; index < parameters.Length; index++)
        {
            AnimatorControllerParameter parameter = parameters[index];

            if (parameter.nameHash == _skillParameterHash &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasSkillAnimation(Animator animator)
    {
        int skillStateFullPathHash =
            animator == _assembledAnimator ?
            _assembledSkillStateFullPathHash :
            _normalSkillStateFullPathHash;

        return
            HasSkillTrigger(animator) &&
            animator.HasState(0, skillStateFullPathHash);
    }

    private void PlaySkillEffect(Animator animator)
    {
        if (!CanUseSkillEffectAnimator(animator))
        {
            return;
        }

        animator.ResetTrigger(_playParameterHash);
        animator.SetTrigger(_playParameterHash);
    }

    private void ResetSkillEffectTriggers()
    {
        ResetSkillEffectTrigger(_normalSkillEffectAnimator);
        ResetSkillEffectTrigger(_assembledSkillEffectAnimator);
    }

    private void ResetAssembledSupportEffectTrigger()
    {
        ResetSkillEffectTrigger(_assembledSupportEffectAnimator);
    }

    private void ResetSkillEffectTrigger(Animator animator)
    {
        if (animator == null ||
            !animator.gameObject.activeInHierarchy ||
            !animator.isActiveAndEnabled ||
            animator.runtimeAnimatorController == null ||
            !HasSkillEffectTrigger(animator))
        {
            return;
        }

        animator.ResetTrigger(_playParameterHash);
    }

    private bool CanUseSkillEffectAnimator(Animator animator)
    {
        if (animator == null ||
            !animator.gameObject.activeInHierarchy ||
            !animator.isActiveAndEnabled ||
            animator.runtimeAnimatorController == null)
        {
            return false;
        }

        return HasSkillEffectTrigger(animator);
    }

    private bool HasSkillEffectTrigger(Animator animator)
    {
        AnimatorControllerParameter[] parameters = animator.parameters;

        for (int index = 0; index < parameters.Length; index++)
        {
            AnimatorControllerParameter parameter = parameters[index];

            if (parameter.nameHash == _playParameterHash &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDisable()
    {
        ResetAssembledSupportEffectTrigger();
        ResetSkillEffectTriggers();
        ResetSkillAnimationState();
    }

    private void SetMoving(bool isMoving)
    {
        if (_isMoving == isMoving)
        {
            return;
        }

        _isMoving = isMoving;
        Animator activeAnimator = _isAssembled ? _assembledAnimator : _normalAnimator;

        if (activeAnimator.gameObject.activeInHierarchy &&
            activeAnimator.isActiveAndEnabled &&
            activeAnimator.runtimeAnimatorController != null)
        {
            activeAnimator.SetBool(_isMovingParameterHash, _isMoving);
        }
    }
}
