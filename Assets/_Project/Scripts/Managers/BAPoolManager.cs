using System.Collections.Generic;
using UnityEngine;

public interface IBAPoolable
{
    void OnSpawned();
    void OnDespawned();
}

[DisallowMultipleComponent]
public class BAPoolManager : MonoBehaviour
{
    private sealed class Pool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly List<GameObject> _instances = new List<GameObject>();

        public GameObject Prefab => _prefab;
        public Transform Parent => _parent;
        public List<GameObject> Instances => _instances;

        public Pool(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }
    }

    private readonly Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();
    private readonly Dictionary<GameObject, Pool> _poolByInstance = new Dictionary<GameObject, Pool>();

    private bool _isInitialized;

    public static BAPoolManager Instance { get; private set; }

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

    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
    }

    public bool RegisterPool(string key, GameObject prefab, int initialSize)
    {
        if (!_isInitialized)
        {
            Debug.LogError("풀 매니저가 초기화되지 않아 풀을 등록할 수 없습니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(key) || prefab == null)
        {
            Debug.LogError("유효하지 않은 키 또는 프리팹으로 풀을 등록할 수 없습니다.");
            return false;
        }

        if (_pools.TryGetValue(key, out Pool registeredPool))
        {
            if (registeredPool.Prefab == prefab)
            {
                return true;
            }

            Debug.LogError($"같은 풀 키에 다른 프리팹이 이미 등록되어 있습니다: {key}");
            return false;
        }

        string parentName = key.Replace('/', '_') + "_Pool";
        GameObject parentObject = new GameObject(parentName);
        parentObject.transform.SetParent(transform, false);

        Pool pool = new Pool(prefab, parentObject.transform);
        _pools.Add(key, pool);

        int preloadCount = Mathf.Max(0, initialSize);

        for (int index = 0; index < preloadCount; index++)
        {
            CreateInstance(pool);
        }

        return true;
    }

    public GameObject Spawn(
        string key,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        if (!_isInitialized)
        {
            Debug.LogError("풀 매니저가 초기화되지 않아 인스턴스를 생성할 수 없습니다.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(key) || !_pools.TryGetValue(key, out Pool pool))
        {
            Debug.LogError($"등록되지 않은 풀 키입니다: {key}");
            return null;
        }

        GameObject instance = null;

        foreach (GameObject pooledInstance in pool.Instances)
        {
            if (pooledInstance != null && !pooledInstance.activeSelf)
            {
                instance = pooledInstance;
                break;
            }
        }

        if (instance == null)
        {
            instance = CreateInstance(pool);
        }

        Transform targetParent = parent != null ? parent : pool.Parent;
        instance.transform.SetParent(targetParent, false);
        instance.transform.localScale = pool.Prefab.transform.localScale;
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        InvokeSpawnedCallbacks(instance);

        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        if (!_poolByInstance.TryGetValue(instance, out Pool pool))
        {
            Debug.LogWarning($"등록된 풀에 속하지 않은 인스턴스입니다: {instance.name}");
            return;
        }

        InvokeDespawnedCallbacks(instance);
        instance.SetActive(false);
        instance.transform.SetParent(pool.Parent, false);
    }

    private GameObject CreateInstance(Pool pool)
    {
        GameObject instance = Instantiate(pool.Prefab, pool.Parent);
        instance.SetActive(false);
        pool.Instances.Add(instance);
        _poolByInstance.Add(instance, pool);
        return instance;
    }

    private void InvokeSpawnedCallbacks(GameObject instance)
    {
        MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IBAPoolable poolable)
            {
                poolable.OnSpawned();
            }
        }
    }

    private void InvokeDespawnedCallbacks(GameObject instance)
    {
        MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IBAPoolable poolable)
            {
                poolable.OnDespawned();
            }
        }
    }

    private void OnDestroy()
    {
        _poolByInstance.Clear();
        _pools.Clear();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
