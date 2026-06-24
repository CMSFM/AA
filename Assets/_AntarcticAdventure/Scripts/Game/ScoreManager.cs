using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private const string BestScoreKey = "AntarcticAdventure_BestScore";
    private const string BestDistanceKey = "AntarcticAdventure_BestDistance";

    [Header("Distance Score")]
    [SerializeField] private int scorePerMeter = 1;

    public int DistanceScore { get; private set; }
    public int ItemScore { get; private set; }
    public int TotalScore => DistanceScore + ItemScore;

    public int CurrentDistanceMeter { get; private set; }
    public int BestScore { get; private set; }
    public int BestDistanceMeter { get; private set; }

    public bool IsNewBestScore { get; private set; }

    private bool submitted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadBestRecords();
    }

    private void Update()
    {
        if (AntarcticGameManager.Instance == null ||
            !AntarcticGameManager.Instance.IsPlaying)
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

        CurrentDistanceMeter = Mathf.FloorToInt(distance);
        DistanceScore = CurrentDistanceMeter * scorePerMeter;
    }

    public void AddItemScore(int amount)
    {
        if (amount <= 0)
            return;

        if (AntarcticGameManager.Instance != null &&
            !AntarcticGameManager.Instance.IsPlaying)
        {
            return;
        }

        ItemScore += amount;

        //Debug.Log($"[Score] Item +{amount}, Total: {TotalScore}");
    }

    public void SubmitCurrentScore()
    {
        if (submitted)
            return;

        submitted = true;

        IsNewBestScore = TotalScore > BestScore;

        if (TotalScore > BestScore)
        {
            BestScore = TotalScore;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
        }

        if (CurrentDistanceMeter > BestDistanceMeter)
        {
            BestDistanceMeter = CurrentDistanceMeter;
            PlayerPrefs.SetInt(BestDistanceKey, BestDistanceMeter);
        }

        PlayerPrefs.Save();

        Debug.Log(
            $"[Score] Submitted. Score: {TotalScore}, Distance: {CurrentDistanceMeter}, Best: {BestScore}"
        );
    }

    private void LoadBestRecords()
    {
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        BestDistanceMeter = PlayerPrefs.GetInt(BestDistanceKey, 0);
    }

    [ContextMenu("Reset Best Records")]
    public void ResetBestRecords()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.DeleteKey(BestDistanceKey);
        PlayerPrefs.Save();

        BestScore = 0;
        BestDistanceMeter = 0;
        IsNewBestScore = false;

        Debug.Log("[Score] Best records reset.");
    }
}