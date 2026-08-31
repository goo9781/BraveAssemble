using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BAMainUIView : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _quitButton;

    private bool _isBound;

    public event Action StartRequested;
    public event Action QuitRequested;

    public bool Bind()
    {
        if (_startButton == null || _quitButton == null)
        {
            Debug.LogError("메인 UI의 시작 및 종료 버튼 참조가 모두 설정되지 않았습니다.");
            return false;
        }

        if (_isBound)
        {
            return true;
        }

        _startButton.onClick.AddListener(OnStartButtonClicked);
        _quitButton.onClick.AddListener(OnQuitButtonClicked);
        _isBound = true;
        return true;
    }

    public void Unbind()
    {
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (_quitButton != null)
        {
            _quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }

        _isBound = false;
    }

    private void OnStartButtonClicked()
    {
        StartRequested?.Invoke();
    }

    private void OnQuitButtonClicked()
    {
        QuitRequested?.Invoke();
    }

    private void OnDestroy()
    {
        Unbind();
        StartRequested = null;
        QuitRequested = null;
    }
}
