using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BALoadingUIView : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Sprite[] _loadingSprites;

    private int _lastSpriteIndex = -1;

    public void ShowRandomImage()
    {
        if (_backgroundImage == null)
        {
            Debug.LogError("Loading UI의 배경 Image 참조가 설정되지 않았습니다.");
            return;
        }

        if (_loadingSprites == null || _loadingSprites.Length == 0)
        {
            Debug.LogWarning("Loading UI에 표시할 Sprite가 설정되지 않았습니다.");
            return;
        }

        List<int> validSpriteIndices = new List<int>();

        for (int index = 0; index < _loadingSprites.Length; index++)
        {
            if (_loadingSprites[index] == null)
            {
                continue;
            }

            if (_loadingSprites.Length > 1 && index == _lastSpriteIndex)
            {
                continue;
            }

            validSpriteIndices.Add(index);
        }

        if (validSpriteIndices.Count == 0 &&
            _lastSpriteIndex >= 0 &&
            _lastSpriteIndex < _loadingSprites.Length &&
            _loadingSprites[_lastSpriteIndex] != null)
        {
            validSpriteIndices.Add(_lastSpriteIndex);
        }

        if (validSpriteIndices.Count == 0)
        {
            Debug.LogWarning("Loading UI에 표시할 유효한 Sprite가 없습니다.");
            return;
        }

        int selectedIndex = validSpriteIndices[Random.Range(0, validSpriteIndices.Count)];
        _lastSpriteIndex = selectedIndex;
        _backgroundImage.sprite = _loadingSprites[selectedIndex];
        _backgroundImage.color = Color.white;
    }
}
