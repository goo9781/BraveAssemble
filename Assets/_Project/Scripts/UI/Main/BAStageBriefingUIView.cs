using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BAStageBriefingUIView : MonoBehaviour
{
    [SerializeField] private TMP_Text _stageNameText;
    [SerializeField] private TMP_Text _enemyInfoText;
    [SerializeField] private TMP_Text _enemyScaleText;
    [SerializeField] private TMP_Text _victoryConditionText;
    [SerializeField] private TMP_Text _recommendedSupportText;
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private Button _startBattleButton;
    [SerializeField] private Button _backButton;

    private bool _isBound;

    public event Action StartBattleRequested;
    public event Action BackRequested;

    public bool Bind()
    {
        if (_stageNameText == null ||
            _enemyInfoText == null ||
            _enemyScaleText == null ||
            _victoryConditionText == null ||
            _recommendedSupportText == null ||
            _rewardText == null ||
            _startBattleButton == null ||
            _backButton == null)
        {
            Debug.LogError("스테이지 브리핑 UI의 Inspector 참조가 모두 설정되지 않았습니다.");
            return false;
        }

        if (_isBound)
        {
            return true;
        }

        _startBattleButton.onClick.AddListener(OnStartBattleButtonClicked);
        _backButton.onClick.AddListener(OnBackButtonClicked);
        _isBound = true;
        return true;
    }

    public void Unbind()
    {
        if (_startBattleButton != null)
        {
            _startBattleButton.onClick.RemoveListener(OnStartBattleButtonClicked);
        }

        if (_backButton != null)
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
        }

        _isBound = false;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetStageInfo(
        string stageName,
        string enemyInfo,
        string enemyScale,
        string victoryCondition,
        string recommendedSupport,
        string rewardInfo)
    {
        _stageNameText.text = stageName;
        _enemyInfoText.text = enemyInfo;
        _enemyScaleText.text = enemyScale;
        _victoryConditionText.text = victoryCondition;
        _recommendedSupportText.text = recommendedSupport;
        _rewardText.text = rewardInfo;
    }

    private void OnStartBattleButtonClicked()
    {
        StartBattleRequested?.Invoke();
    }

    private void OnBackButtonClicked()
    {
        BackRequested?.Invoke();
    }

    private void OnDestroy()
    {
        Unbind();
        StartBattleRequested = null;
        BackRequested = null;
    }
}
