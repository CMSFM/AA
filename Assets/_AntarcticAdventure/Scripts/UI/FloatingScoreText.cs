using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class FloatingScoreText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text text;

    [Header("Motion")]
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private float moveUpDistance = 80f;
    [SerializeField] private float startScale = 1.15f;
    [SerializeField] private float endScale = 0.9f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 startAnchoredPosition;
    private float elapsedTime;
    private bool isPlaying;

    private Action<FloatingScoreText> onFinished;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    public void Play(
        string message,
        Vector2 anchoredPosition,
        Action<FloatingScoreText> finishCallback
    )
    {
        onFinished = finishCallback;

        gameObject.SetActive(true);

        if (text != null)
            text.text = message;

        startAnchoredPosition = anchoredPosition;

        rectTransform.anchoredPosition = startAnchoredPosition;
        rectTransform.localScale = Vector3.one * startScale;

        canvasGroup.alpha = 1f;

        elapsedTime = 0f;
        isPlaying = true;
    }

    public void StopImmediately()
    {
        isPlaying = false;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        elapsedTime += Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(elapsedTime / Mathf.Max(0.001f, duration));
        float easedT = 1f - Mathf.Pow(1f - t, 2f);

        rectTransform.anchoredPosition =
            startAnchoredPosition + Vector2.up * moveUpDistance * easedT;

        float scale = Mathf.Lerp(startScale, endScale, easedT);
        rectTransform.localScale = Vector3.one * scale;

        canvasGroup.alpha = 1f - t;

        if (t >= 1f)
        {
            Finish();
        }
    }

    private void Finish()
    {
        isPlaying = false;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        onFinished?.Invoke(this);
    }
}