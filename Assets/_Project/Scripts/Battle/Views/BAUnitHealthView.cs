using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BAUnitView))]
public class BAUnitHealthView : MonoBehaviour
{
    [SerializeField] private TMP_Text _healthText;

    private BAUnitView _unitView;

    private void Awake()
    {
        _unitView = GetComponent<BAUnitView>();

        if (_healthText == null)
        {
            Debug.LogError("유닛 체력 TMP_Text 참조가 설정되지 않았습니다.");
        }
        else
        {
            UpdateHealthText(0f, 0f);
        }

        _unitView.HealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthText(currentHealth, maxHealth);
    }

    private void UpdateHealthText(float currentHealth, float maxHealth)
    {
        if (_healthText == null)
        {
            return;
        }

        float clampedCurrentHealth = Mathf.Max(0f, currentHealth);
        float clampedMaxHealth = Mathf.Max(0f, maxHealth);
        _healthText.text = $"{clampedCurrentHealth:F0} / {clampedMaxHealth:F0}";
    }

    private void OnDestroy()
    {
        if (_unitView != null)
        {
            _unitView.HealthChanged -= OnHealthChanged;
        }
    }
}
