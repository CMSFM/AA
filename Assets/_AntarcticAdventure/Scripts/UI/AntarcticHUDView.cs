using TMPro;
using UnityEngine;

public class AntarcticHUDView : MonoBehaviour
{
    [Header("HUD Texts")]
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Ready")]
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private TMP_Text readyBestText;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverResultText;

    private bool resultShown;

    private void Start()
    {
        SetReadyPanel(true);
        SetGameOverPanel(false);
    }

    private void Update()
    {
        UpdateDistance();
        UpdateSpeed();
        UpdateScore();

        UpdateReadyPanel();
        UpdateGameOverPanel();
    }

    private void UpdateDistance()
    {
        if (distanceText == null)
            return;

        int distance = ScoreManager.Instance != null
            ? ScoreManager.Instance.CurrentDistanceMeter
            : 0;

        distanceText.text = $"DIST {distance:0000} m";
    }

    private void UpdateSpeed()
    {
        if (speedText == null)
            return;

        float speed = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.CurrentSpeed
            : 0f;

        speedText.text = $"SPEED {speed:0.0}";
    }

    private void UpdateScore()
    {
        if (scoreText == null)
            return;

        int score = ScoreManager.Instance != null
            ? ScoreManager.Instance.TotalScore
            : 0;

        scoreText.text = $"SCORE {score:000000}";
    }

    private void UpdateReadyPanel()
    {
        bool isReady =
            AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsReady;

        SetReadyPanel(isReady);

        if (isReady)
            UpdateReadyBestText();
    }

    private void UpdateReadyBestText()
    {
        if (readyBestText == null)
            return;

        int bestScore = ScoreManager.Instance != null
            ? ScoreManager.Instance.BestScore
            : 0;

        int bestDistance = ScoreManager.Instance != null
            ? ScoreManager.Instance.BestDistanceMeter
            : 0;

        readyBestText.text =
            $"BEST SCORE {bestScore:000000}   BEST DIST {bestDistance:0000} m";
    }

    private void UpdateGameOverPanel()
    {
        bool isGameOver =
            AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsGameOver;

        SetGameOverPanel(isGameOver);

        if (isGameOver && !resultShown)
        {
            resultShown = true;
            UpdateGameOverResult();
        }
    }

    private void UpdateGameOverResult()
    {
        if (gameOverResultText == null)
            return;

        if (ScoreManager.Instance == null)
        {
            gameOverResultText.text = "NO SCORE DATA";
            return;
        }

        ScoreManager score = ScoreManager.Instance;

        string newBestText = score.IsNewBestScore
            ? "\nNEW BEST!"
            : "";

        gameOverResultText.text =
            $"DISTANCE {score.CurrentDistanceMeter:0000} m\n" +
            $"SCORE {score.TotalScore:000000}\n" +
            $"ITEM SCORE {score.ItemScore:000000}\n" +
            $"BEST SCORE {score.BestScore:000000}" +
            newBestText;
    }

    private void SetReadyPanel(bool isActive)
    {
        if (readyPanel == null)
            return;

        if (readyPanel.activeSelf == isActive)
            return;

        readyPanel.SetActive(isActive);
    }

    private void SetGameOverPanel(bool isActive)
    {
        if (gameOverPanel == null)
            return;

        if (gameOverPanel.activeSelf == isActive)
            return;

        gameOverPanel.SetActive(isActive);
    }
}