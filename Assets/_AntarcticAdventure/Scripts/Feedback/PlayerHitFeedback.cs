using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitFeedback : MonoBehaviour
{
    private class RendererMaterialCache
    {
        public Renderer targetRenderer;
        public Material[] originalMaterials;
        public Material[] hitMaterials;
    }

    [Header("References")]
    [SerializeField] private Renderer[] renderers;

    [Header("Hit Material Flash")]
    [SerializeField] private Material hitFlashMaterial;
    [SerializeField] private float flashDuration = 0.35f;
    [SerializeField] private int flashCount = 3;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private readonly List<RendererMaterialCache> materialCaches = new List<RendererMaterialCache>();
    private Coroutine flashCoroutine;

    private bool hasLoggedMissingMaterial;
    private bool hasLoggedMissingRenderers;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        CacheRendererMaterials();
    }

    private void CacheRendererMaterials()
    {
        materialCaches.Clear();

        if (renderers == null || renderers.Length == 0)
        {
            LogMissingRenderersOnce();
            return;
        }

        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Renderer targetRenderer = renderers[rendererIndex];

            if (targetRenderer == null)
                continue;

            Material[] originalMaterials = targetRenderer.sharedMaterials;

            if (originalMaterials == null || originalMaterials.Length == 0)
                continue;

            Material[] hitMaterials = new Material[originalMaterials.Length];

            for (int materialIndex = 0; materialIndex < hitMaterials.Length; materialIndex++)
            {
                hitMaterials[materialIndex] = hitFlashMaterial;
            }

            RendererMaterialCache cache = new RendererMaterialCache
            {
                targetRenderer = targetRenderer,
                originalMaterials = originalMaterials,
                hitMaterials = hitMaterials
            };

            materialCaches.Add(cache);
        }
    }

    public void PlayHitFeedback()
    {
        if (hitFlashMaterial == null)
        {
            LogMissingMaterialOnce();
            return;
        }

        if (materialCaches.Count == 0)
        {
            CacheRendererMaterials();

            if (materialCaches.Count == 0)
                return;
        }

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float singleFlashTime = flashDuration / Mathf.Max(1, flashCount * 2);

        for (int i = 0; i < flashCount; i++)
        {
            ApplyHitMaterials();
            yield return new WaitForSecondsRealtime(singleFlashTime);

            RestoreMaterials();
            yield return new WaitForSecondsRealtime(singleFlashTime);
        }

        RestoreMaterials();
        flashCoroutine = null;
    }

    private void ApplyHitMaterials()
    {
        for (int i = 0; i < materialCaches.Count; i++)
        {
            RendererMaterialCache cache = materialCaches[i];

            if (cache.targetRenderer == null)
                continue;

            cache.targetRenderer.sharedMaterials = cache.hitMaterials;
        }
    }

    private void RestoreMaterials()
    {
        for (int i = 0; i < materialCaches.Count; i++)
        {
            RendererMaterialCache cache = materialCaches[i];

            if (cache.targetRenderer == null)
                continue;

            cache.targetRenderer.sharedMaterials = cache.originalMaterials;
        }
    }

    private void LogMissingMaterialOnce()
    {
        if (hasLoggedMissingMaterial)
            return;

        hasLoggedMissingMaterial = true;

        Debug.LogError(
            "[PlayerHitFeedback] Hit Flash Material이 연결되지 않았습니다. " +
            "PlayerHitFeedback에 빨간 피격용 Material을 직접 연결하세요.",
            this
        );
    }

    private void LogMissingRenderersOnce()
    {
        if (hasLoggedMissingRenderers)
            return;

        hasLoggedMissingRenderers = true;

        Debug.LogError(
            "[PlayerHitFeedback] Renderer가 없습니다. " +
            "PlayerHitFeedback의 Renderers 배열에 캐릭터 Renderer들을 직접 연결하세요.",
            this
        );
    }

    private void OnDisable()
    {
        RestoreMaterials();
    }
}