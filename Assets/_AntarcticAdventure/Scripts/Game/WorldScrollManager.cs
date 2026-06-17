using UnityEngine;

public class WorldScrollManager : MonoBehaviour
{
    public static WorldScrollManager Instance { get; private set; }

    [Header("Speed")]
    [SerializeField] private float baseSpeed = 6f;
    [SerializeField] private float speedIncreasePerSecond = 0.05f;
    [SerializeField] private float maxSpeed = 14f;

    public float CurrentSpeed { get; private set; }
    public float Distance { get; private set; }

    private float elapsedTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentSpeed = baseSpeed;
    }

    private void Update()
    {
        if (AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsGameOver)
        {
            CurrentSpeed = 0f;
            return;
        }

        elapsedTime += Time.deltaTime;

        CurrentSpeed = Mathf.Min(
            maxSpeed,
            baseSpeed + elapsedTime * speedIncreasePerSecond
        );

        Distance += CurrentSpeed * Time.deltaTime;
    }
}