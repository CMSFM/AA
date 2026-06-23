using UnityEngine;

[System.Serializable]
public class DifficultyStage
{
    [Header("Stage")]
    public string stageName = "Easy";

    [Tooltip("이 거리부터 이 난이도 단계가 시작된다.")]
    public float startDistance = 0f;

    [Header("World Speed")]
    public float worldSpeed = 6f;

    [Header("Spawn")]
    [Tooltip("패턴 간격 배율. 낮을수록 패턴이 자주 나온다.")]
    public float spawnDistanceMultiplier = 1f;
}

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Stages")]
    [SerializeField] private DifficultyStage[] stages;

    [Header("Fallback")]
    [SerializeField] private float fallbackSpeed = 6f;
    [SerializeField] private float fallbackSpawnDistanceMultiplier = 1f;

    [Header("Debug")]
    [SerializeField] private bool showStageChangeLog;

    public string CurrentStageName { get; private set; }
    public int CurrentStageIndex { get; private set; }

    private int previousStageIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    
        Instance = this;
        CurrentStageIndex = 0;
    
        if (stages != null && stages.Length > 0 && stages[0] != null)
            CurrentStageName = stages[0].stageName;
        else
            CurrentStageName = "Easy";
    }

    private void Update()
    {
        if (AntarcticGameManager.Instance == null ||
            !AntarcticGameManager.Instance.IsPlaying)
        {
            return;
        }

        float distance = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.Distance
            : 0f;

        CurrentStageIndex = GetStageIndex(distance);

        if (stages != null &&
            stages.Length > 0 &&
            CurrentStageIndex >= 0 &&
            CurrentStageIndex < stages.Length)
        {
            CurrentStageName = stages[CurrentStageIndex].stageName;
        }

        if (showStageChangeLog && CurrentStageIndex != previousStageIndex)
        {
            previousStageIndex = CurrentStageIndex;
            Debug.Log($"[Difficulty] Stage changed: {CurrentStageName}");
        }
    }

    public float GetWorldSpeed(float distance)
    {
        DifficultyStage currentStage = GetStage(distance);

        if (currentStage == null)
            return fallbackSpeed;

        DifficultyStage nextStage = GetNextStage(distance);

        if (nextStage == null)
            return currentStage.worldSpeed;

        float t = Mathf.InverseLerp(
            currentStage.startDistance,
            nextStage.startDistance,
            distance
        );

        return Mathf.Lerp(
            currentStage.worldSpeed,
            nextStage.worldSpeed,
            t
        );
    }

    public float GetSpawnDistanceMultiplier(float distance)
    {
        DifficultyStage currentStage = GetStage(distance);

        if (currentStage == null)
            return fallbackSpawnDistanceMultiplier;

        DifficultyStage nextStage = GetNextStage(distance);

        if (nextStage == null)
            return currentStage.spawnDistanceMultiplier;

        float t = Mathf.InverseLerp(
            currentStage.startDistance,
            nextStage.startDistance,
            distance
        );

        return Mathf.Lerp(
            currentStage.spawnDistanceMultiplier,
            nextStage.spawnDistanceMultiplier,
            t
        );
    }

    private DifficultyStage GetStage(float distance)
    {
        int index = GetStageIndex(distance);

        if (stages == null || stages.Length == 0)
            return null;

        index = Mathf.Clamp(index, 0, stages.Length - 1);

        return stages[index];
    }

    private DifficultyStage GetNextStage(float distance)
    {
        if (stages == null || stages.Length == 0)
            return null;

        int currentIndex = GetStageIndex(distance);
        int nextIndex = currentIndex + 1;

        if (nextIndex < 0 || nextIndex >= stages.Length)
            return null;

        return stages[nextIndex];
    }

    private int GetStageIndex(float distance)
    {
        if (stages == null || stages.Length == 0)
            return -1;

        int result = 0;

        for (int i = 0; i < stages.Length; i++)
        {
            if (distance >= stages[i].startDistance)
                result = i;
            else
                break;
        }

        return result;
    }
}