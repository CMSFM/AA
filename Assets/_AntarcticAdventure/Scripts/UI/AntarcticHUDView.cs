using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AntarcticHUDView : MonoBehaviour
{
    [Header("HUD Texts")]
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text difficultyText;

    [Header("Ready")]
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private TMP_Text readyBestText;
    
    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    
    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverResultText;

    [Header("Ranking")]
    [SerializeField] private TMP_Text rankingText;
    [SerializeField] private GameObject nameInputGroup;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button registerRankingButton;
    [SerializeField] private TMP_Text rankingGuideText;
    [SerializeField] private int rankingDisplayCount = 5;

    private bool resultShown;
    private bool rankingPrepared;
    private bool rankingRegistered;

    private void Start()
    {
        SetReadyPanel(true);
        SetGameOverPanel(false);

        if (registerRankingButton != null)
            registerRankingButton.onClick.AddListener(RegisterCurrentScoreToRanking);
    }

    private void OnDestroy()
    {
        if (registerRankingButton != null)
            registerRankingButton.onClick.RemoveListener(RegisterCurrentScoreToRanking);
    }

    private void Update()
    {
        UpdateDistance();
        UpdateSpeed();
        UpdateScore();
        UpdateDifficulty();

        UpdateReadyPanel();
        UpdatePausePanel();
        UpdateGameOverPanel();
        UpdateRankingInputShortcut();
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

    private void UpdateDifficulty()
    {
        if (difficultyText == null)
            return;
    
        string stageName = "READY";
    
        if (AntarcticGameManager.Instance != null)
        {
            if (AntarcticGameManager.Instance.IsReady)
            {
                stageName = "READY";
            }
            else if (AntarcticGameManager.Instance.IsPaused)
            {
                stageName = "PAUSED";
            }
            else if (AntarcticGameManager.Instance.IsGameOver)
            {
                stageName = "GAME OVER";
            }
            else if (DifficultyManager.Instance != null &&
                     !string.IsNullOrEmpty(DifficultyManager.Instance.CurrentStageName))
            {
                stageName = DifficultyManager.Instance.CurrentStageName;
            }
        }
    
        difficultyText.text = stageName.ToUpper();
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

        if (isGameOver && !rankingPrepared)
        {
            rankingPrepared = true;
            PrepareRankingInput();
            RefreshRankingText();
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

    private void PrepareRankingInput()
    {
        rankingRegistered = false;

        if (nameInputGroup != null)
            nameInputGroup.SetActive(true);

        if (playerNameInput != null)
        {
            string lastName = RankingManager.Instance != null
                ? RankingManager.Instance.LastPlayerName
                : "PLAYER";

            playerNameInput.text = lastName;
            playerNameInput.interactable = true;
            playerNameInput.Select();
            playerNameInput.ActivateInputField();
        }

        if (registerRankingButton != null)
            registerRankingButton.interactable = true;

        if (rankingGuideText != null)
            rankingGuideText.text = "이름 입력 후 등록하세요";
    }

    private void UpdateRankingInputShortcut()
    {
        if (rankingRegistered)
            return;

        if (AntarcticGameManager.Instance == null ||
            !AntarcticGameManager.Instance.IsGameOver)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
            RegisterCurrentScoreToRanking();
    }

    public void RegisterCurrentScoreToRanking()
    {
        if (rankingRegistered)
            return;

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("[HUD] ScoreManager가 없어 랭킹 등록을 할 수 없습니다.", this);
            return;
        }

        if (RankingManager.Instance == null)
        {
            Debug.LogError("[HUD] RankingManager가 없어 랭킹 등록을 할 수 없습니다.", this);
            return;
        }

        string playerName = playerNameInput != null
            ? playerNameInput.text
            : "PLAYER";

        ScoreManager score = ScoreManager.Instance;

        RankingManager.Instance.RegisterScore(
            playerName,
            score.TotalScore,
            score.CurrentDistanceMeter,
            score.ItemScore
        );

        rankingRegistered = true;

        if (playerNameInput != null)
            playerNameInput.interactable = false;

        if (registerRankingButton != null)
            registerRankingButton.interactable = false;

        if (rankingGuideText != null)
            rankingGuideText.text = "랭킹 등록 완료";

        RefreshRankingText();
    }

    private void RefreshRankingText()
    {
        if (rankingText == null)
            return;

        string ranking = RankingManager.Instance != null
            ? RankingManager.Instance.GetRankingText(rankingDisplayCount)
            : "NO RANKING DATA";

        rankingText.text =
            "RANKING\n\n" +
            ranking;
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
    private void UpdatePausePanel()
    {
        bool isPaused =
            AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsPaused;

        SetPausePanel(isPaused);
    }

    private void SetPausePanel(bool isActive)
    {
        if (pausePanel == null)
            return;

        if (pausePanel.activeSelf == isActive)
            return;

        pausePanel.SetActive(isActive);
    }
}