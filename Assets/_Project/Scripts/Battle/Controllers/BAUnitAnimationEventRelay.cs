using UnityEngine;

[DisallowMultipleComponent]
public class BAUnitAnimationEventRelay : MonoBehaviour
{
    [SerializeField] private BAUnitCombatController _unitCombatController;

    private void Awake()
    {
        if (_unitCombatController == null)
        {
            Debug.LogError("Animation Event를 전달할 BAUnitCombatController 참조가 설정되지 않았습니다.");
        }
    }

    public void AnimationEvent_ApplyAttackDamage()
    {
        if (_unitCombatController == null)
        {
            return;
        }

        _unitCombatController.ApplyPendingAttackDamage();
    }
}
