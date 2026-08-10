using UnityEngine;

[System.Serializable]
public class BAUnitData
{
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private string _unitType;
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _detectionRange;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackInterval;
    [SerializeField] private string _prefabKey;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string UnitType => _unitType;
    public float MaxHealth => _maxHealth;
    public float AttackDamage => _attackDamage;
    public float MoveSpeed => _moveSpeed;
    public float DetectionRange => _detectionRange;
    public float AttackRange => _attackRange;
    public float AttackInterval => _attackInterval;
    public string PrefabKey => _prefabKey;
}
