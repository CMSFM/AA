using UnityEngine;
using UnityEngine.SceneManagement;

public class AntarcticGameManager : MonoBehaviour
{
    public static AntarcticGameManager Instance { get; private set; }

    [Header("Game Over")]
    [SerializeField] private bool pauseOnGameOver = true;

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!IsGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    public void GameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;

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