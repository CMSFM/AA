using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AntarcticPlayerController playerController;
    [SerializeField] private Transform visualRoot;

    [Header("Normal Visual")]
    [SerializeField] private Vector3 normalLocalPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 normalLocalScale = new Vector3(1f, 1f, 1f);

    [Header("Slide Visual")]
    [SerializeField] private Vector3 slideLocalPosition = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private Vector3 slideLocalScale = new Vector3(1f, 0.5f, 1f);

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 14f;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<AntarcticPlayerController>();

        if (visualRoot == null && transform.childCount > 0)
            visualRoot = transform.GetChild(0);
    }

    private void Update()
    {
        if (visualRoot == null || playerController == null)
            return;

        bool isSliding = playerController.IsSliding;

        Vector3 targetPosition = isSliding
            ? slideLocalPosition
            : normalLocalPosition;

        Vector3 targetScale = isSliding
            ? slideLocalScale
            : normalLocalScale;

        visualRoot.localPosition = Vector3.Lerp(
            visualRoot.localPosition,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            targetScale,
            smoothSpeed * Time.deltaTime
        );
    }
}