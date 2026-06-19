using TMPro;
using UnityEngine;

public class AntarcticHUDView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        SetGameOverPanel(false);
    }

    private void Update()
    {
        UpdateDistance();
        UpdateSpeed();
        UpdateScore();
        UpdateGameOverPanel();
    }

    private void UpdateDistance()
    {
        if (distanceText == null)
            return;

        float distance = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.Distance
            : 0f;

        int distanceInt = Mathf.FloorToInt(distance);

        distanceText.text = $"DIST {distanceInt:0000} m";
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

    private void UpdateGameOverPanel()
    {
        bool isGameOver =
            AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsGameOver;

        SetGameOverPanel(isGameOver);
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