using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private WorldPoolManager ownerPool;
    private GameObject sourcePrefab;

    public bool HasPool => ownerPool != null && sourcePrefab != null;

    public void Initialize(WorldPoolManager pool, GameObject prefab)
    {
        ownerPool = pool;
        sourcePrefab = prefab;
    }

    public void Release()
    {
        if (!HasPool)
        {
            Destroy(gameObject);
            return;
        }

        ownerPool.ReturnToPool(sourcePrefab, this);
    }
}