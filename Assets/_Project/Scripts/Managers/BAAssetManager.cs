using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[DisallowMultipleComponent]
public class BAAssetManager : MonoBehaviour
{
    private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabLoadHandles =
        new Dictionary<string, AsyncOperationHandle<GameObject>>();

    private bool _isInitialized;
    private bool _isInitializing;

    public static BAAssetManager Instance { get; private set; }

    public bool IsInitialized => _isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator InitializeAsync()
    {
        if (_isInitialized)
        {
            yield break;
        }

        if (_isInitializing)
        {
            while (_isInitializing)
            {
                yield return null;
            }

            yield break;
        }

        _isInitializing = true;
        AsyncOperationHandle initializationHandle = Addressables.InitializeAsync(false);
        yield return initializationHandle;

        bool initializationSucceeded =
            initializationHandle.Status == AsyncOperationStatus.Succeeded;

        if (!initializationSucceeded)
        {
            Debug.LogError("Addressables 초기화에 실패했습니다.");
        }

        if (initializationHandle.IsValid())
        {
            Addressables.Release(initializationHandle);
        }

        _isInitialized = initializationSucceeded;
        _isInitializing = false;
    }

    public IEnumerator LoadPrefabAsync(string key, Action<GameObject> onCompleted)
    {
        if (!_isInitialized)
        {
            Debug.LogError("에셋 매니저가 초기화되지 않아 프리팹을 불러올 수 없습니다.");
            onCompleted?.Invoke(null);
            yield break;
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogError("프리팹 Addressables 키가 비어 있습니다.");
            onCompleted?.Invoke(null);
            yield break;
        }

        if (_prefabLoadHandles.TryGetValue(key, out AsyncOperationHandle<GameObject> cachedHandle))
        {
            if (!cachedHandle.IsDone)
            {
                yield return cachedHandle;
            }

            if (cachedHandle.Status == AsyncOperationStatus.Succeeded && cachedHandle.Result != null)
            {
                onCompleted?.Invoke(cachedHandle.Result);
                yield break;
            }

            Debug.LogError($"캐시된 프리팹 로드 작업이 실패했습니다: {key}");
            onCompleted?.Invoke(null);
            yield break;
        }

        AsyncOperationHandle<GameObject> loadHandle = Addressables.LoadAssetAsync<GameObject>(key);
        _prefabLoadHandles.Add(key, loadHandle);
        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded && loadHandle.Result != null)
        {
            onCompleted?.Invoke(loadHandle.Result);
            yield break;
        }

        Debug.LogError($"프리팹을 불러오지 못했습니다: {key}");
        onCompleted?.Invoke(null);
    }

    private void OnDestroy()
    {
        foreach (AsyncOperationHandle<GameObject> loadHandle in _prefabLoadHandles.Values)
        {
            if (loadHandle.IsValid())
            {
                Addressables.Release(loadHandle);
            }
        }

        _prefabLoadHandles.Clear();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
