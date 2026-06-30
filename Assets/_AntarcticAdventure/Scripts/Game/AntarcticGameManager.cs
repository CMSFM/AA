using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum AntarcticGameState
{
    Ready,
    Playing,
    Paused,
    GameOver
}

public class AntarcticGameManager : MonoBehaviour
{
    public static AntarcticGameManager Instance { get; private set; }

    [Header("Start")]
    [SerializeField] private KeyCode startKey = KeyCode.Return;
    [SerializeField] private bool allowSpaceToStart = false;

    [Header("Pause")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
[Header("UI")]
[SerializeField] private AntarcticHUDView hudView;
    [Header("Game Over")]
    [SerializeField] private bool pauseOnGameOver = true;
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [Header("Start Flow")]
    [SerializeField] private InputProviderSwitcher inputProviderSwitcher;
    [SerializeField] private CountdownView countdownView;
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private bool calibrateMocapOnStart = true;
    [SerializeField] private bool useCountdown = true;

    private Coroutine startRoutine;
    private bool hasLoggedMissingInputProviderSwitcher;
    private bool hasLoggedMissingCountdownView;
    private bool hasLoggedMissingReadyPanel;
    public AntarcticGameState CurrentState { get; private set; }

    public bool IsReady => CurrentState == AntarcticGameState.Ready;
    public bool IsPlaying => CurrentState == AntarcticGameState.Playing;
    public bool IsPaused => CurrentState == AntarcticGameState.Paused;
    public bool IsGameOver => CurrentState == AntarcticGameState.GameOver;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentState = AntarcticGameState.Ready;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case AntarcticGameState.Ready:
                UpdateReady();
                break;

            case AntarcticGameState.Playing:
                UpdatePlaying();
                break;

            case AntarcticGameState.Paused:
                UpdatePaused();
                break;

            case AntarcticGameState.GameOver:
                UpdateGameOver();
                break;
        }
    }

    private void UpdateReady()
    {
        if (Input.GetKeyDown(startKey) ||
            (allowSpaceToStart && Input.GetKeyDown(KeyCode.Space)))
        {
            StartGame();
        }
    }

    private void UpdatePlaying()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            PauseGame();
        }
    }

    private void UpdatePaused()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            ResumeGame();
            return;
        }

        if (Input.GetKeyDown(restartKey))
        {
            Restart();
        }
    }

    private void UpdateGameOver()
    {
        if (Input.GetKeyDown(restartKey))
        {
            Restart();
        }
    }

    public void StartGame()
    {
        if (CurrentState != AntarcticGameState.Ready)
            return;

        if (startRoutine != null)
            return;

        startRoutine = StartCoroutine(StartGameRoutine());
    } 

    public void PauseGame()
    {
        if (!IsPlaying)
            return;

        CurrentState = AntarcticGameState.Paused;
        Time.timeScale = 0f;

        Debug.Log("[GameManager] Pause.");
    }

    public void ResumeGame()
    {
        if (!IsPaused)
            return;

        CurrentState = AntarcticGameState.Playing;
        Time.timeScale = 1f;

        Debug.Log("[GameManager] Resume.");
    }

    public void GameOver()
    {
        if (IsGameOver)
            return;

        CurrentState = AntarcticGameState.GameOver;

        if (GameFeedbackManager.Instance != null)
            GameFeedbackManager.Instance.PlayHitFeedback();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.SubmitCurrentScore();

        Debug.Log("Game Over! Press R to restart.");

        if (pauseOnGameOver)
            Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    private void LogMissingInputProviderSwitcherOnce()
    {
        if (hasLoggedMissingInputProviderSwitcher)
            return;

        hasLoggedMissingInputProviderSwitcher = true;

        Debug.LogError(
            "[AntarcticGameManager] InputProviderSwitcher가 연결되지 않았습니다. " +
            "GameManager의 AntarcticGameManager에서 Player의 InputProviderSwitcher를 직접 연결하세요.",
            this
        );
    }

    private void LogMissingCountdownViewOnce()
    {
        if (hasLoggedMissingCountdownView)
            return;

        hasLoggedMissingCountdownView = true;

        Debug.LogError(
            "[AntarcticGameManager] CountdownView가 연결되지 않았습니다. " +
            "GameManager의 AntarcticGameManager에서 GameCanvas의 CountdownView를 직접 연결하세요.",
            this
        );
    }

    private void LogMissingReadyPanelOnce()
    {
        if (hasLoggedMissingReadyPanel)
            return;

        hasLoggedMissingReadyPanel = true;

        Debug.LogError(
            "[AntarcticGameManager] ReadyPanel이 연결되지 않았습니다. " +
            "GameManager의 AntarcticGameManager에서 ReadyPanel GameObject를 직접 연결하세요.",
            this
        );
    }
    
    private IEnumerator StartGameRoutine()
    {
        Time.timeScale = 1f;
    
        if (calibrateMocapOnStart)
        {
            if (inputProviderSwitcher != null)
            {
                inputProviderSwitcher.CalibrateCurrentInputIfMocap();
            }
            else
            {
                LogMissingInputProviderSwitcherOnce();
            }
        }
    
        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }
        else
        {
            LogMissingReadyPanelOnce();
        }
    
        if (useCountdown)
        {
            if (countdownView != null)
            {
                yield return countdownView.PlayCountdown();
            }
            else
            {
                LogMissingCountdownViewOnce();
            }
        }
    
        CurrentState = AntarcticGameState.Playing;
    
        startRoutine = null;
    }
}