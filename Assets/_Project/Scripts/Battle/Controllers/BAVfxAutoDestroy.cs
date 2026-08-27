using UnityEngine;

[DisallowMultipleComponent]
public class BAVfxAutoDestroy : MonoBehaviour, IBAPoolable
{
    private Animator _animator;
    private bool _isDestroyRequested;
    private bool _isPooledInstance;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnSpawned()
    {
        _isDestroyRequested = false;
        _isPooledInstance = true;

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }
    }

    public void OnDespawned()
    {
    }

    public void AnimationEvent_DestroyVfx()
    {
        if (_isDestroyRequested)
        {
            return;
        }

        _isDestroyRequested = true;

        if (_isPooledInstance && BAPoolManager.Instance != null)
        {
            BAPoolManager.Instance.Release(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
