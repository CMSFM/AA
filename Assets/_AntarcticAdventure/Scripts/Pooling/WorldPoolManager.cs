using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldPoolDefinition
{
    public GameObject prefab;
    public int initialSize = 5;
    public int maxSize = 30;
    public bool allowExpand = true;
}

public class WorldPoolManager : MonoBehaviour
{
    public static WorldPoolManager Instance { get; private set; }

    [Header("Pool Root")]
    [SerializeField] private Transform poolRoot;

    [Header("Pool Definitions")]
    [SerializeField] private WorldPoolDefinition[] poolDefinitions;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private class PoolData
    {
        public GameObject prefab;
        public Queue<PooledObject> inactiveObjects = new Queue<PooledObject>();
        public int createdCount;
        public int maxSize;
        public bool allowExpand;
        public bool hasLoggedLimit;
    }

    private readonly Dictionary<GameObject, PoolData> pools = new Dictionary<GameObject, PoolData>();

    private bool hasLoggedMissingRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (poolRoot == null)
        {
            poolRoot = transform;
        }

        BuildInitialPools();
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Spawn(prefab, position, rotation, null);
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null)
            return null;

        PoolData pool = GetOrCreatePool(prefab);

        PooledObject pooledObject = GetFromPool(pool);

        if (pooledObject == null)
            return null;

        Transform objectTransform = pooledObject.transform;

        objectTransform.SetParent(parent);
        objectTransform.SetPositionAndRotation(position, rotation);

        pooledObject.gameObject.SetActive(true);

        return pooledObject.gameObject;
    }

    public void ReturnToPool(GameObject prefab, PooledObject pooledObject)
    {
        if (prefab == null || pooledObject == null)
            return;

        PoolData pool = GetOrCreatePool(prefab);

        pooledObject.gameObject.SetActive(false);
        pooledObject.transform.SetParent(poolRoot);
        pooledObject.transform.localPosition = Vector3.zero;
        pooledObject.transform.localRotation = Quaternion.identity;

        pool.inactiveObjects.Enqueue(pooledObject);
    }

    private void BuildInitialPools()
    {
        if (poolDefinitions == null)
            return;

        for (int i = 0; i < poolDefinitions.Length; i++)
        {
            WorldPoolDefinition definition = poolDefinitions[i];

            if (definition == null || definition.prefab == null)
                continue;

            PoolData pool = GetOrCreatePool(definition.prefab);

            pool.maxSize = Mathf.Max(0, definition.maxSize);
            pool.allowExpand = definition.allowExpand;

            int initialSize = Mathf.Max(0, definition.initialSize);

            for (int j = 0; j < initialSize; j++)
            {
                PooledObject pooledObject = CreatePooledObject(pool);
                ReturnToPool(pool.prefab, pooledObject);
            }
        }
    }

    private PoolData GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out PoolData existingPool))
            return existingPool;

        WorldPoolDefinition definition = FindDefinition(prefab);

        PoolData newPool = new PoolData
        {
            prefab = prefab,
            maxSize = definition != null ? Mathf.Max(0, definition.maxSize) : 30,
            allowExpand = definition == null || definition.allowExpand
        };

        pools.Add(prefab, newPool);

        return newPool;
    }

    private WorldPoolDefinition FindDefinition(GameObject prefab)
    {
        if (poolDefinitions == null)
            return null;

        for (int i = 0; i < poolDefinitions.Length; i++)
        {
            WorldPoolDefinition definition = poolDefinitions[i];

            if (definition != null && definition.prefab == prefab)
                return definition;
        }

        return null;
    }

    private PooledObject GetFromPool(PoolData pool)
    {
        while (pool.inactiveObjects.Count > 0)
        {
            PooledObject pooledObject = pool.inactiveObjects.Dequeue();

            if (pooledObject != null)
                return pooledObject;
        }

        if (!pool.allowExpand)
        {
            LogPoolLimitOnce(pool);
            return null;
        }

        if (pool.maxSize > 0 && pool.createdCount >= pool.maxSize)
        {
            LogPoolLimitOnce(pool);
            return null;
        }

        return CreatePooledObject(pool);
    }

    private PooledObject CreatePooledObject(PoolData pool)
    {
        if (pool.prefab == null)
            return null;

        GameObject instance = Instantiate(
            pool.prefab,
            poolRoot
        );

        pool.createdCount++;

        PooledObject pooledObject = instance.GetComponent<PooledObject>();

        if (pooledObject == null)
            pooledObject = instance.AddComponent<PooledObject>();

        pooledObject.Initialize(this, pool.prefab);
        instance.SetActive(false);

        if (showDebugLog)
        {
            Debug.Log(
                $"[WorldPoolManager] Created pooled object: {pool.prefab.name}, Count: {pool.createdCount}",
                this
            );
        }

        return pooledObject;
    }

    private void LogPoolLimitOnce(PoolData pool)
    {
        if (pool.hasLoggedLimit)
            return;

        pool.hasLoggedLimit = true;

        Debug.LogWarning(
            $"[WorldPoolManager] Pool limit reached: {pool.prefab.name}. Max Size를 늘리거나 Allow Expand를 켜주세요.",
            this
        );
    }

    private void LogMissingRootOnce()
    {
        if (hasLoggedMissingRoot)
            return;

        hasLoggedMissingRoot = true;

        Debug.LogError(
            "[WorldPoolManager] Pool Root가 연결되지 않았습니다.",
            this
        );
    }
}