using UnityEngine;
public class WorldScrollManager : MonoBehaviour
{
    public static WorldScrollManager Instance { get; private set; }

    [Header("Fallback Speed")]
    [SerializeField] private float fallbackSpeed = 6f;

    public float CurrentSpeed { get; private set; }
    public float Distance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentSpeed = 0f;
    }

    private void Update()
    {
        if (AntarcticGameManager.Instance == null ||
            !AntarcticGameManager.Instance.IsPlaying)
        {
            CurrentSpeed = 0f;
            return;
        }

        CurrentSpeed = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.GetWorldSpeed(Distance)
            : fallbackSpeed;

        Distance += CurrentSpeed * Time.deltaTime;
    }

    public void SetDistanceForDebug(float distance)
    {
        Distance = Mathf.Max(0f, distance);
    }
}