using UnityEngine;

public class FloatingScoreTextSpawner : MonoBehaviour
{
    public static FloatingScoreTextSpawner Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private FloatingScoreText floatingScoreTextPrefab;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);

    private Camera mainCamera;
    private RectTransform canvasRectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (targetCanvas == null)
            targetCanvas = GetComponentInParent<Canvas>();

        if (targetCanvas != null)
            canvasRectTransform = targetCanvas.GetComponent<RectTransform>();

        if (popupRoot == null && targetCanvas != null)
            popupRoot = targetCanvas.transform as RectTransform;

        mainCamera = Camera.main;
    }

    public void ShowWorldText(string message, Vector3 worldPosition)
    {
        if (floatingScoreTextPrefab == null || popupRoot == null || canvasRectTransform == null)
            return;

        Vector3 screenPosition = mainCamera != null
            ? mainCamera.WorldToScreenPoint(worldPosition + worldOffset)
            : worldPosition;

        if (screenPosition.z < 0f)
            return;

        Camera uiCamera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
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

        FloatingScoreText popup = Instantiate(
            floatingScoreTextPrefab,
            popupRoot
        );

        popup.Play(message, anchoredPosition);
    }
}