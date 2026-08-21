using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BAUnitView))]
public class BAHeroVisualController : MonoBehaviour
{
    [SerializeField] private GameObject _normalVisual;
    [SerializeField] private GameObject _assembledVisual;

    private bool _isInitialized;
    private bool _isAssembled;

    public bool IsInitialized => _isInitialized;
    public bool IsAssembled => _isAssembled;

    private void Awake()
    {
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

        _isInitialized = true;
        TrySetAssembled(false);
    }

    public bool TrySetAssembled(bool isAssembled)
    {
        if (!_isInitialized)
        {
            return false;
        }

        _normalVisual.SetActive(!isAssembled);
        _assembledVisual.SetActive(isAssembled);
        _isAssembled = isAssembled;
        return true;
    }
}
