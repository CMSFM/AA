using UnityEngine;

public class PlayerSlideVFXController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AntarcticPlayerController playerController;
    [SerializeField] private Transform slideVfxAnchor;

    private ParticleSystem slideVfxInstance;
    private bool wasSliding;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<AntarcticPlayerController>();

        if (slideVfxAnchor == null)
            slideVfxAnchor = transform;
    }

    private void Update()
    {
        if (playerController == null)
            return;

        bool isSliding =
            AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsPlaying &&
            playerController.IsSliding;

        if (isSliding && !wasSliding)
            StartSlideVfx();

        if (!isSliding && wasSliding)
            StopSlideVfx();

        wasSliding = isSliding;
    }

    private void StartSlideVfx()
    {
        if (slideVfxInstance == null)
        {
            if (AntarcticVFXManager.Instance == null)
                return;

            slideVfxInstance = AntarcticVFXManager.Instance.SpawnSlideSnow(slideVfxAnchor);

            if (slideVfxInstance == null)
                return;
        }

        slideVfxInstance.Play();
    }

    private void StopSlideVfx()
    {
        if (slideVfxInstance == null)
            return;

        slideVfxInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}