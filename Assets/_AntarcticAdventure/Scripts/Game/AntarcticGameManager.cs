using UnityEngine;
using UnityEngine.SceneManagement;

public enum AntarcticGameState
{
    Ready,
    Playing,
    GameOver
}

public class AntarcticGameManager : MonoBehaviour
{
    public static AntarcticGameManager Instance { get; private set; }

    [Header("Start")]
    [SerializeField] private KeyCode startKey = KeyCode.Return;
    [SerializeField] private bool allowSpaceToStart = false;

    [Header("Game Over")]
    [SerializeField] private bool pauseOnGameOver = true;
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    public AntarcticGameState CurrentState { get; private set; }

    public bool IsReady => CurrentState == AntarcticGameState.Ready;
    public bool IsPlaying => CurrentState == AntarcticGameState.Playing;
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

    private void UpdateGameOver()
    {
        if (Input.GetKeyDown(restartKey))
        {
            Restart();
        }
    }

    public void StartGame()
    {
        if (!IsReady)
            return;

        CurrentState = AntarcticGameState.Playing;
        Time.timeScale = 1f;

        Debug.Log("[GameManager] Game Start.");
    }

    public void GameOver()
    {
        if (IsGameOver)
            return;

        CurrentState = AntarcticGameState.GameOver;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.SubmitCurrentScore();

        Debug.Log("Game Over! Press R to restart.");

        if (pauseOnGameOver)
            Time.timeScale = 0f;
    }

    private void Restart()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}