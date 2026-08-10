using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BAUnitTableData
{
    [SerializeField] private List<BAUnitData> _units = new List<BAUnitData>();

    public IReadOnlyList<BAUnitData> Units => _units;
}
