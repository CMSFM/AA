using UnityEngine;

public class GameFeedbackManager : MonoBehaviour
{
    public static GameFeedbackManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private PlayerHitFeedback playerHitFeedback;

    [Header("Hit Feedback")]
    [SerializeField] private float hitShakeDuration = 0.25f;
    [SerializeField] private float hitShakeMagnitude = 0.18f;

    private bool hasLoggedMissingCameraShake;
    private bool hasLoggedMissingPlayerHitFeedback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (cameraShake == null)
        {
            Debug.LogError(
                "[GameFeedbackManager] CameraShake가 연결되지 않았습니다. " +
                "GameManager의 GameFeedbackManager 컴포넌트에서 Main Camera의 CameraShake를 직접 연결하세요.",
                this
            );
        }

        if (playerHitFeedback == null)
        {
            Debug.LogError(
                "[GameFeedbackManager] PlayerHitFeedback이 연결되지 않았습니다. " +
                "GameManager의 GameFeedbackManager 컴포넌트에서 Player의 PlayerHitFeedback을 직접 연결하세요.",
                this
            );
        }
    }

    public void PlayHitFeedback()
    {
        if (AntarcticAudioManager.Instance != null)
            AntarcticAudioManager.Instance.PlayHit();   

        if (AntarcticVFXManager.Instance != null && playerHitFeedback != null)
            AntarcticVFXManager.Instance.PlayHit(playerHitFeedback.transform.position + Vector3.up * 0.8f); 

        PlayCameraShake();
        PlayPlayerHitFeedback();
    }

    private void PlayCameraShake()
    {
        if (cameraShake == null)
        {
            if (!hasLoggedMissingCameraShake)
            {
                hasLoggedMissingCameraShake = true;

                Debug.LogError(
                    "[GameFeedbackManager] 충돌 피드백 실행 실패: CameraShake가 없습니다.",
                    this
                );
            }

            return;
        }

        cameraShake.Play(hitShakeDuration, hitShakeMagnitude);
    }

    private void PlayPlayerHitFeedback()
    {
        if (playerHitFeedback == null)
        {
            if (!hasLoggedMissingPlayerHitFeedback)
            {
                hasLoggedMissingPlayerHitFeedback = true;

                Debug.LogError(
                    "[GameFeedbackManager] 충돌 피드백 실행 실패: PlayerHitFeedback이 없습니다.",
                    this
                );
            }

            return;
        }

        playerHitFeedback.PlayHitFeedback();
    }
}