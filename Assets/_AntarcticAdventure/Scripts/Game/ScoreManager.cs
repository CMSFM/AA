using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField] private int scorePerMeter = 1;

    public int ItemScore { get; private set; }
    public int DistanceScore { get; private set; }
    public int TotalScore => DistanceScore + ItemScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsGameOver)
        {
            return;
        }

        UpdateDistanceScore();
    }

    private void UpdateDistanceScore()
    {
        float distance = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.Distance
            : 0f;

        DistanceScore = Mathf.FloorToInt(distance) * scorePerMeter;
    }

    public void AddItemScore(int amount)
    {
        if (amount <= 0)
            return;

        ItemScore += amount;

        Debug.Log($"[Score] Item +{amount}, Total: {TotalScore}");
    }
}