using System.Collections;
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
            LogMissingRootOnce();

        ParticleSystem instance = SpawnParticle(prefab, position, rotation, parent);

        if (instance == null)
            return;

        instance.gameObject.SetActive(true);
        instance.transform.SetPositionAndRotation(position, rotation);

        if (parent != null)
            instance.transform.SetParent(parent);

        instance.Clear(true);
        instance.Play(true);

        StartCoroutine(ReturnOneShotVfxRoutine(instance));
    }

    private ParticleSystem SpawnParticle(
        ParticleSystem prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent
    )
    {
        if (WorldPoolManager.Instance != null)
        {
            GameObject pooledObject = WorldPoolManager.Instance.Spawn(
                prefab.gameObject,
                position,
                rotation,
                parent
            );

            if (pooledObject != null &&
                pooledObject.TryGetComponent(out ParticleSystem pooledParticle))
            {
                return pooledParticle;
            }
        }

        return Instantiate(
            prefab,
            position,
            rotation,
            parent
        );
    }

    private IEnumerator ReturnOneShotVfxRoutine(ParticleSystem instance)
    {
        if (instance == null)
            yield break;
    
        ParticleSystem.MainModule main = instance.main;
    
        float duration =
            main.duration +
            main.startLifetime.constantMax +
            0.2f;
    
        yield return new WaitForSecondsRealtime(duration);
    
        if (instance == null)
            yield break;
    
        PooledObject pooledObject = instance.GetComponent<PooledObject>();
    
        if (pooledObject != null && pooledObject.HasPool)
        {
            instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            pooledObject.Release();
            yield break;
        }
    
        Destroy(instance.gameObject);
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