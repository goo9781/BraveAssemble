using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BASkillManager : MonoBehaviour
{
    private const string _heroUnitType = "Hero";
    private const string _areaDamageSkillType = "AreaDamage";

    [SerializeField] private string _skillId;

    private BABattleManager _battleManager;
    private BAStageManager _stageManager;
    private BAAssembleManager _assembleManager;
    private BASkillModel _skillModel;
    private bool _isInitialized;

    public static BASkillManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;
    public string DisplayName => _skillModel?.DisplayName;
    public float Cooldown => _skillModel?.Cooldown ?? 0f;
    public float RemainingCooldown => _skillModel?.RemainingCooldown ?? 0f;
    public bool CanUse =>
        _isInitialized &&
        _skillModel != null &&
        _skillModel.CanUse &&
        !_stageManager.IsStageEnded;

    public event Action<float, float> CooldownChanged;
    public event Action<int> SkillUsed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool Initialize(
        BADataManager dataManager,
        BABattleManager battleManager,
        BAStageManager stageManager,
        BAAssembleManager assembleManager)
    {
        if (_isInitialized)
        {
            return true;
        }

        if (dataManager == null || !dataManager.IsInitialized)
        {
            Debug.LogError("BADataManager가 없거나 초기화되지 않아 스킬 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (battleManager == null || !battleManager.IsInitialized)
        {
            Debug.LogError("BABattleManager가 없거나 초기화되지 않아 스킬 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (stageManager == null || !stageManager.IsInitialized)
        {
            Debug.LogError("BAStageManager가 없거나 초기화되지 않아 스킬 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (assembleManager == null || !assembleManager.IsInitialized)
        {
            Debug.LogError("BAAssembleManager가 없거나 초기화되지 않아 스킬 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_skillId))
        {
            Debug.LogError("스킬 ID가 설정되지 않아 스킬 매니저를 초기화할 수 없습니다.");
            return false;
        }

        if (!dataManager.TryGetSkillData(_skillId, out BASkillData skillData))
        {
            Debug.LogError($"스킬 데이터를 찾을 수 없습니다: {_skillId}");
            return false;
        }

        if (skillData.SkillType != _areaDamageSkillType)
        {
            Debug.LogError($"지원하지 않는 스킬 유형입니다: {skillData.SkillType}");
            return false;
        }

        if (skillData.DamageMultiplier <= 0f ||
            skillData.Range <= 0f ||
            skillData.MaxTargetCount <= 0 ||
            skillData.AssembledDamageMultiplier <= 0f ||
            skillData.AssembledRange <= 0f ||
            skillData.AssembledMaxTargetCount <= 0 ||
            skillData.Cooldown < 0f)
        {
            Debug.LogError($"스킬 데이터의 수치가 유효하지 않습니다: {_skillId}");
            return false;
        }

        _battleManager = battleManager;
        _stageManager = stageManager;
        _assembleManager = assembleManager;
        _skillModel = new BASkillModel(skillData);
        _skillModel.CooldownChanged += OnCooldownChanged;
        _isInitialized = true;
        CooldownChanged?.Invoke(_skillModel.RemainingCooldown, _skillModel.Cooldown);
        return true;
    }

    private void Update()
    {
        if (!_isInitialized || _skillModel == null)
        {
            return;
        }

        _skillModel.UpdateCooldown(Time.deltaTime);
    }

    public bool TryUseSkill()
    {
        if (!_isInitialized || !CanUse)
        {
            return false;
        }

        if (!_battleManager.TryGetFirstActiveUnitByType(
                _heroUnitType,
                out BAUnitView heroView))
        {
            return false;
        }

        float damageMultiplier = _assembleManager.IsAssembled
            ? _skillModel.AssembledDamageMultiplier
            : _skillModel.DamageMultiplier;
        float range = _assembleManager.IsAssembled
            ? _skillModel.AssembledRange
            : _skillModel.Range;
        int maxTargetCount = _assembleManager.IsAssembled
            ? _skillModel.AssembledMaxTargetCount
            : _skillModel.MaxTargetCount;
        float damage = heroView.AttackDamage * damageMultiplier;

        if (!_battleManager.TryApplyAreaDamage(
                heroView,
                range,
                maxTargetCount,
                damage,
                out int hitCount))
        {
            return false;
        }

        _skillModel.TryStartCooldown();
        SkillUsed?.Invoke(hitCount);
        return true;
    }

    private void OnCooldownChanged(float remainingCooldown, float cooldown)
    {
        CooldownChanged?.Invoke(remainingCooldown, cooldown);
    }

    private void OnDestroy()
    {
        if (_skillModel != null)
        {
            _skillModel.CooldownChanged -= OnCooldownChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
