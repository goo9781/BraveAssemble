using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BAUnitView))]
public class BAUnitHitFlashController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] _renderers;
    [SerializeField] private Color _flashColor;
    [SerializeField, Min(0f)] private float _flashDuration;

    private BAUnitView _unitView;
    private Color[] _originalColors;
    private Coroutine _flashCoroutine;
    private float _previousHealth;
    private bool _hasReceivedHealth;

    private void Awake()
    {
        _unitView = GetComponent<BAUnitView>();

        if (_renderers == null)
        {
            _renderers = new SpriteRenderer[0];
        }

        _originalColors = new Color[_renderers.Length];

        for (int index = 0; index < _renderers.Length; index++)
        {
            if (_renderers[index] == null)
            {
                continue;
            }

            _originalColors[index] = _renderers[index].color;
        }
    }

    private void OnEnable()
    {
        if (_unitView == null)
        {
            return;
        }

        _unitView.HealthChanged -= OnHealthChanged;
        _unitView.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (_unitView != null)
        {
            _unitView.HealthChanged -= OnHealthChanged;
        }

        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        RestoreOriginalColors();
        _previousHealth = 0f;
        _hasReceivedHealth = false;
    }

    private void OnDestroy()
    {
        if (_unitView != null)
        {
            _unitView.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        if (!_hasReceivedHealth)
        {
            _previousHealth = currentHealth;
            _hasReceivedHealth = true;
            return;
        }

        bool hasTakenDamage = currentHealth < _previousHealth;
        _previousHealth = currentHealth;

        if (!hasTakenDamage)
        {
            return;
        }

        StartFlash();
    }

    private void StartFlash()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        RestoreOriginalColors();

        for (int index = 0; index < _renderers.Length; index++)
        {
            if (_renderers[index] == null)
            {
                continue;
            }

            _renderers[index].color = _flashColor;
        }

        if (_flashDuration <= 0f)
        {
            RestoreOriginalColors();
            return;
        }

        _flashCoroutine = StartCoroutine(FlashAsync());
    }

    private IEnumerator FlashAsync()
    {
        yield return new WaitForSeconds(_flashDuration);

        RestoreOriginalColors();
        _flashCoroutine = null;
    }

    private void RestoreOriginalColors()
    {
        for (int index = 0; index < _renderers.Length; index++)
        {
            if (_renderers[index] == null)
            {
                continue;
            }

            _renderers[index].color = _originalColors[index];
        }
    }
}
