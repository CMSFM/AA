using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScoreItem : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private int scoreAmount = 100;

    [Header("Visual")]
    [SerializeField] private float rotateSpeed = 180f;

    private bool collected;
    private PooledObject pooledObject;

    private void Reset()
    {
        Collider itemCollider = GetComponent<Collider>();

        if (itemCollider != null)
            itemCollider.isTrigger = true;
    }

    private void Awake()
    {
        CachePooledObject();

        Collider itemCollider = GetComponent<Collider>();

        if (itemCollider != null)
            itemCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        collected = false;
        CachePooledObject();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (AntarcticGameManager.Instance == null ||
            !AntarcticGameManager.Instance.IsPlaying)
        {
            return;
        }

        AntarcticPlayerController player =
            other.GetComponentInParent<AntarcticPlayerController>();

        if (player == null)
            return;

        Collect();
    }

    private void Collect()
    {
        collected = true;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddItemScore(scoreAmount);

        if (AntarcticAudioManager.Instance != null)
            AntarcticAudioManager.Instance.PlayItemGet();

        if (AntarcticVFXManager.Instance != null)
            AntarcticVFXManager.Instance.PlayItemCollect(transform.position);

        if (FloatingScoreTextSpawner.Instance != null)
            FloatingScoreTextSpawner.Instance.ShowWorldText($"+{scoreAmount}", transform.position);

        ReleaseOrDestroy();
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