using UnityEngine;

public class AntarcticAudioManager : MonoBehaviour
{
    public static AntarcticAudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip slideStartClip;
    [SerializeField] private AudioClip slideEndClip;
    [SerializeField] private AudioClip itemGetClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip uiClickClip;
    [SerializeField] private AudioClip rankingRegisterClip;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float masterSfxVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float jumpVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float slideVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float itemVolume = 0.9f;

    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float uiVolume = 0.7f;

    private bool hasLoggedMissingAudioSource;

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
        if (sfxAudioSource == null)
        {
            LogMissingAudioSourceOnce();
        }
    }

    public void PlayJump()
    {
        PlayOneShot(jumpClip, jumpVolume);
    }

    public void PlaySlideStart()
    {
        PlayOneShot(slideStartClip, slideVolume);
    }

    public void PlaySlideEnd()
    {
        PlayOneShot(slideEndClip, slideVolume);
    }

    public void PlayItemGet()
    {
        PlayOneShot(itemGetClip, itemVolume);
    }

    public void PlayHit()
    {
        PlayOneShot(hitClip, hitVolume);
    }

    public void PlayUiClick()
    {
        PlayOneShot(uiClickClip, uiVolume);
    }

    public void PlayRankingRegister()
    {
        PlayOneShot(rankingRegisterClip, uiVolume);
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (sfxAudioSource == null)
        {
            LogMissingAudioSourceOnce();
            return;
        }

        if (clip == null)
            return;

        float finalVolume = Mathf.Clamp01(masterSfxVolume * volume);

        sfxAudioSource.PlayOneShot(clip, finalVolume);
    }

    private void LogMissingAudioSourceOnce()
    {
        if (hasLoggedMissingAudioSource)
            return;

        hasLoggedMissingAudioSource = true;

        Debug.LogError(
            "[AntarcticAudioManager] SFX AudioSource가 연결되지 않았습니다. " +
            "GameManager의 AntarcticAudioManager 컴포넌트에서 AudioSource를 직접 연결하세요.",
            this
        );
    }
}