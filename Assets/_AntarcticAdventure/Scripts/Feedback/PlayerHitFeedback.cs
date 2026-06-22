using System.Collections;
using UnityEngine;

public class PlayerHitFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer[] renderers;

    [Header("Flash")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.35f;
    [SerializeField] private int flashCount = 3;

    private Color[] originalColors;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        CacheOriginalColors();
    }

    private void CacheOriginalColors()
    {
        if (renderers == null)
            return;

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null)
                continue;

            originalColors[i] = renderers[i].material.color;
        }
    }

    public void PlayHitFeedback()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float singleFlashTime = flashDuration / Mathf.Max(1, flashCount * 2);

        for (int i = 0; i < flashCount; i++)
        {
            SetColor(hitColor);
            yield return new WaitForSecondsRealtime(singleFlashTime);

            RestoreColor();
            yield return new WaitForSecondsRealtime(singleFlashTime);
        }

        RestoreColor();
        flashCoroutine = null;
    }

    private void SetColor(Color color)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null)
                continue;

            renderers[i].material.color = color;
        }
    }

    private void RestoreColor()
    {
        if (renderers == null || originalColors == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null)
                continue;

            renderers[i].material.color = originalColors[i];
        }
    }
}