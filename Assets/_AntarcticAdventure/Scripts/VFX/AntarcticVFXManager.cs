using UnityEngine;

public class AntarcticVFXManager : MonoBehaviour
{
    public static AntarcticVFXManager Instance { get; private set; }

    [Header("VFX Prefabs")]
    [SerializeField] private ParticleSystem itemCollectVfxPrefab;
    [SerializeField] private ParticleSystem hitVfxPrefab;
    [SerializeField] private ParticleSystem slideSnowVfxPrefab;

    [Header("Parents")]
    [SerializeField] private Transform vfxRoot;

    private bool hasLoggedMissingRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (vfxRoot == null)
            LogMissingRootOnce();
    }

    public void PlayItemCollect(Vector3 position)
    {
        PlayOneShot(itemCollectVfxPrefab, position, Quaternion.identity);
    }

    public void PlayHit(Vector3 position)
    {
        PlayOneShot(hitVfxPrefab, position, Quaternion.identity);
    }

    public ParticleSystem SpawnSlideSnow(Transform parent)
    {
        if (slideSnowVfxPrefab == null)
            return null;

        if (parent == null)
            return null;

        ParticleSystem instance = Instantiate(
            slideSnowVfxPrefab,
            parent
        );

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        return instance;
    }

    private void PlayOneShot(ParticleSystem prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return;

        Transform parent = vfxRoot;

        if (parent == null)
        {
            LogMissingRootOnce();
        }

        ParticleSystem instance = Instantiate(
            prefab,
            position,
            rotation,
            parent
        );

        instance.Play();

        float duration = instance.main.duration + instance.main.startLifetime.constantMax + 0.2f;
        Destroy(instance.gameObject, duration);
    }

    private void LogMissingRootOnce()
    {
        if (hasLoggedMissingRoot)
            return;

        hasLoggedMissingRoot = true;

        Debug.LogError(
            "[AntarcticVFXManager] VFX Root가 연결되지 않았습니다. GameManager의 AntarcticVFXManager에서 VFXRoot를 직접 연결하세요.",
            this
        );
    }
}