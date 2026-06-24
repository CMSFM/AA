using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitFeedback : MonoBehaviour
{
    private class MaterialFlashCache
    {
        public Material material;

        public string colorPropertyName;
        public Color originalColor;

        public string outlineWidthPropertyName;
        public float originalOutlineWidth;
        public bool hasOutlineWidth;
    }

    [Header("References")]
    [SerializeField] private Renderer[] renderers;

    [Header("Flash")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.35f;
    [SerializeField] private int flashCount = 3;

    [Header("Outline Flash")]
    [SerializeField] private float hitOutlineWidth = 0.035f;

    [Header("Debug")]
    [SerializeField] private bool showSkippedMaterialLog;

    private readonly List<MaterialFlashCache> materialCaches = new List<MaterialFlashCache>();
    private Coroutine flashCoroutine;

    private static readonly string[] ColorPropertyNames =
    {
        "_ASEOutlineColor",
        "_BaseColor",
        "_Color",
        "_TintColor",
        "_OutlineColor",
        "_Outline_Color",
        "_OutlineColour"
    };
    
    private static readonly string[] OutlineWidthPropertyNames =
    {
        "_ASEOutlineWidth",
        "_OutlineWidth",
        "_Outline_Width",
        "_OutlineSize",
        "_Outline_Size"
    };

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        CacheMaterials();
    }

    private void CacheMaterials()
    {
        materialCaches.Clear();

        if (renderers == null)
            return;

        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Renderer targetRenderer = renderers[rendererIndex];

            if (targetRenderer == null)
                continue;

            Material[] materials = targetRenderer.materials;

            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];

                if (material == null)
                    continue;

                string colorPropertyName = FindPropertyName(material, ColorPropertyNames);
                string outlineWidthPropertyName = FindPropertyName(material, OutlineWidthPropertyNames);

                bool hasColor = !string.IsNullOrEmpty(colorPropertyName);
                bool hasOutlineWidth = !string.IsNullOrEmpty(outlineWidthPropertyName);

                if (!hasColor && !hasOutlineWidth)
                {
                    if (showSkippedMaterialLog)
                    {
                        Debug.Log(
                            $"[PlayerHitFeedback] 피격 이펙트에 사용할 색상/외곽선 프로퍼티가 없습니다. Material: {material.name}, Shader: {material.shader.name}",
                            this
                        );
                    }

                    continue;
                }

                MaterialFlashCache cache = new MaterialFlashCache
                {
                    material = material,
                    colorPropertyName = colorPropertyName,
                    outlineWidthPropertyName = outlineWidthPropertyName,
                    hasOutlineWidth = hasOutlineWidth
                };

                if (hasColor)
                    cache.originalColor = material.GetColor(colorPropertyName);

                if (hasOutlineWidth)
                    cache.originalOutlineWidth = material.GetFloat(outlineWidthPropertyName);

                materialCaches.Add(cache);
            }
        }
    }

    private string FindPropertyName(Material material, string[] propertyNames)
    {
        for (int i = 0; i < propertyNames.Length; i++)
        {
            string propertyName = propertyNames[i];

            if (material.HasProperty(propertyName))
                return propertyName;
        }

        return "";
    }

    public void PlayHitFeedback()
    {
        if (materialCaches.Count == 0)
            return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float singleFlashTime = flashDuration / Mathf.Max(1, flashCount * 2);

        for (int i = 0; i < flashCount; i++)
        {
            ApplyHitFlash();
            yield return new WaitForSecondsRealtime(singleFlashTime);

            RestoreMaterials();
            yield return new WaitForSecondsRealtime(singleFlashTime);
        }

        RestoreMaterials();
        flashCoroutine = null;
    }

    private void ApplyHitFlash()
    {
        for (int i = 0; i < materialCaches.Count; i++)
        {
            MaterialFlashCache cache = materialCaches[i];

            if (cache.material == null)
                continue;

            if (!string.IsNullOrEmpty(cache.colorPropertyName) &&
                cache.material.HasProperty(cache.colorPropertyName))
            {
                cache.material.SetColor(cache.colorPropertyName, hitColor);
            }

            if (cache.hasOutlineWidth &&
                cache.material.HasProperty(cache.outlineWidthPropertyName))
            {
                cache.material.SetFloat(cache.outlineWidthPropertyName, hitOutlineWidth);
            }
        }
    }

    private void RestoreMaterials()
    {
        for (int i = 0; i < materialCaches.Count; i++)
        {
            MaterialFlashCache cache = materialCaches[i];

            if (cache.material == null)
                continue;

            if (!string.IsNullOrEmpty(cache.colorPropertyName) &&
                cache.material.HasProperty(cache.colorPropertyName))
            {
                cache.material.SetColor(cache.colorPropertyName, cache.originalColor);
            }

            if (cache.hasOutlineWidth &&
                cache.material.HasProperty(cache.outlineWidthPropertyName))
            {
                cache.material.SetFloat(cache.outlineWidthPropertyName, cache.originalOutlineWidth);
            }
        }
    }
}