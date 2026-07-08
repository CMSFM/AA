using System.Collections.Generic;
using UnityEngine;

public class FloatingScoreTextSpawner : MonoBehaviour
{
    public static FloatingScoreTextSpawner Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private FloatingScoreText floatingScoreTextPrefab;
    [SerializeField] private Camera worldCamera;

    [Header("Pool")]
    [SerializeField] private int initialPoolSize = 12;
    [SerializeField] private bool allowPoolExpand = true;
    [SerializeField] private int maxPoolSize = 30;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);

    private readonly Queue<FloatingScoreText> pool = new Queue<FloatingScoreText>();
    private RectTransform canvasRectTransform;

    private bool hasLoggedMissingCanvas;
    private bool hasLoggedMissingPopupRoot;
    private bool hasLoggedMissingPrefab;
    private bool hasLoggedMissingCamera;
    private bool hasLoggedPoolLimit;

    private int createdCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ValidateReferences();

        if (targetCanvas != null)
            canvasRectTransform = targetCanvas.GetComponent<RectTransform>();

        PrewarmPool();
    }

    public void ShowWorldText(string message, Vector3 worldPosition)
    {
        if (!CanShowText())
            return;

        Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition + worldOffset);

        if (screenPosition.z < 0f)
            return;

        Camera uiCamera = targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? targetCanvas.worldCamera
            : null;

        bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPosition,
            uiCamera,
            out Vector2 anchoredPosition
        );

        if (!converted)
            return;

        FloatingScoreText popup = GetPopupFromPool();

        if (popup == null)
            return;

        popup.Play(message, anchoredPosition, ReturnToPool);
    }

    private void PrewarmPool()
    {
        if (floatingScoreTextPrefab == null || popupRoot == null)
            return;

        int count = Mathf.Max(0, initialPoolSize);

        for (int i = 0; i < count; i++)
        {
            FloatingScoreText popup = CreatePopup();
            ReturnToPool(popup);
        }
    }

    private FloatingScoreText GetPopupFromPool()
    {
        while (pool.Count > 0)
        {
            FloatingScoreText popup = pool.Dequeue();

            if (popup != null)
                return popup;
        }

        if (!allowPoolExpand)
        {
            LogPoolLimitOnce();
            return null;
        }

        if (maxPoolSize > 0 && createdCount >= maxPoolSize)
        {
            LogPoolLimitOnce();
            return null;
        }

        return CreatePopup();
    }

    private FloatingScoreText CreatePopup()
    {
        if (floatingScoreTextPrefab == null || popupRoot == null)
            return null;

        FloatingScoreText popup = Instantiate(
            floatingScoreTextPrefab,
            popupRoot
        );

        createdCount++;

        popup.StopImmediately();

        return popup;
    }

    private void ReturnToPool(FloatingScoreText popup)
    {
        if (popup == null)
            return;

        popup.gameObject.SetActive(false);
        pool.Enqueue(popup);
    }

    private bool CanShowText()
    {
        bool canShow = true;

        if (targetCanvas == null)
        {
            LogMissingCanvasOnce();
            canShow = false;
        }

        if (popupRoot == null)
        {
            LogMissingPopupRootOnce();
            canShow = false;
        }

        if (floatingScoreTextPrefab == null)
        {
            LogMissingPrefabOnce();
            canShow = false;
        }

        if (worldCamera == null)
        {
            LogMissingCameraOnce();
            canShow = false;
        }

        if (canvasRectTransform == null && targetCanvas != null)
            canvasRectTransform = targetCanvas.GetComponent<RectTransform>();

        if (canvasRectTransform == null)
            canShow = false;

        return canShow;
    }

    private void ValidateReferences()
    {
        if (targetCanvas == null)
            LogMissingCanvasOnce();

        if (popupRoot == null)
            LogMissingPopupRootOnce();

        if (floatingScoreTextPrefab == null)
            LogMissingPrefabOnce();

        if (worldCamera == null)
            LogMissingCameraOnce();
    }

    private void LogMissingCanvasOnce()
    {
        if (hasLoggedMissingCanvas)
            return;

        hasLoggedMissingCanvas = true;

        Debug.LogError(
            "[FloatingScoreTextSpawner] Target Canvas가 연결되지 않았습니다. " +
            "GameCanvas의 FloatingScoreTextSpawner에서 Target Canvas를 직접 연결하세요.",
            this
        );
    }

    private void LogMissingPopupRootOnce()
    {
        if (hasLoggedMissingPopupRoot)
            return;

        hasLoggedMissingPopupRoot = true;

        Debug.LogError(
            "[FloatingScoreTextSpawner] Popup Root가 연결되지 않았습니다. " +
            "점수 팝업이 들어갈 RectTransform을 직접 연결하세요.",
            this
        );
    }

    private void LogMissingPrefabOnce()
    {
        if (hasLoggedMissingPrefab)
            return;

        hasLoggedMissingPrefab = true;

        Debug.LogError(
            "[FloatingScoreTextSpawner] Floating Score Text Prefab이 연결되지 않았습니다.",
            this
        );
    }

    private void LogMissingCameraOnce()
    {
        if (hasLoggedMissingCamera)
            return;

        hasLoggedMissingCamera = true;

        Debug.LogError(
            "[FloatingScoreTextSpawner] World Camera가 연결되지 않았습니다. " +
            "Main Camera를 직접 연결하세요.",
            this
        );
    }

    private void LogPoolLimitOnce()
    {
        if (hasLoggedPoolLimit)
            return;

        hasLoggedPoolLimit = true;

        Debug.LogWarning(
            "[FloatingScoreTextSpawner] Floating Score Text Pool이 부족합니다. " +
            "Initial Pool Size 또는 Max Pool Size를 늘려주세요.",
            this
        );
    }
}