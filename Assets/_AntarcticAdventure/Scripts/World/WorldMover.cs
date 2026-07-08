using UnityEngine;

public class WorldMover : MonoBehaviour
{
    [Header("Pool Return")]
    [SerializeField] private bool releaseWhenBehind = true;
    [SerializeField] private float releaseZ = -15f;

    private PooledObject pooledObject;

    private void Awake()
    {
        CachePooledObject();
    }

    private void OnEnable()
    {
        CachePooledObject();
    }

    private void Update()
    {
        float speed = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.CurrentSpeed
            : 0f;

        if (speed <= 0f)
            return;

        transform.position += Vector3.back * speed * Time.deltaTime;

        if (releaseWhenBehind && transform.position.z <= releaseZ)
        {
            ReleaseOrDestroy();
        }
    }

    private void CachePooledObject()
    {
        if (pooledObject != null)
            return;

        pooledObject = GetComponent<PooledObject>();
    }

    private void ReleaseOrDestroy()
    {
        CachePooledObject();

        if (pooledObject != null && pooledObject.HasPool)
        {
            pooledObject.Release();
            return;
        }

        Destroy(gameObject);
    }
}