using UnityEngine;

[System.Serializable]
public class PatternSpawnElement
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Lane")]
    [Tooltip("0 = 왼쪽, 1 = 중앙, 2 = 오른쪽")]
    public int laneIndex = 1;

    [Header("Position Offset")]
    public float y = 0f;
    public float zOffset = 0f;
}

[System.Serializable]
public class SpawnPattern
{
    [Header("Pattern Info")]
    public string patternName = "New Pattern";

    [Tooltip("이 패턴이 뽑힐 가중치")]
    public float weight = 1f;

    [Tooltip("이 패턴 이후 다음 패턴까지의 최소 거리")]
    public float minNextDistance = 10f;

    [Tooltip("이 패턴 이후 다음 패턴까지의 최대 거리")]
    public float maxNextDistance = 16f;

    [Header("Difficulty")]
    [Tooltip("이 거리 이후부터 등장 가능")]
    public float unlockDistance = 0f;

    [Header("Elements")]
    public PatternSpawnElement[] elements;
}

public class PatternSpawner : MonoBehaviour
{
    [Header("Patterns")]
    [SerializeField] private SpawnPattern[] patterns;

    [Header("Spawn Position")]
    [SerializeField] private float spawnZ = 35f;
    [SerializeField] private float[] laneXPositions = new float[] { -3f, 0f, 3f };

    [Header("Start")]
    [SerializeField] private float initialSpawnDistance = 8f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private float distanceUntilNextSpawn;

    private void Start()
    {
        distanceUntilNextSpawn = initialSpawnDistance;
    }

    private void Update()
    {
        if (AntarcticGameManager.Instance == null ||
            !AntarcticGameManager.Instance.IsPlaying)
        {
            return;
        }
    
        float speed = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.CurrentSpeed
            : 0f;
    
        if (speed <= 0f)
            return;
    
        distanceUntilNextSpawn -= speed * Time.deltaTime;
    
        if (distanceUntilNextSpawn <= 0f)
        {
            SpawnPattern pattern = PickPattern();
    
            if (pattern != null)
            {
                Spawn(pattern);
                ResetNextSpawnDistance(pattern);
            }
            else
            {
                distanceUntilNextSpawn = 5f;
            }
        }
    }

    private SpawnPattern PickPattern()
    {
        if (patterns == null || patterns.Length == 0)
            return null;

        float currentDistance = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.Distance
            : 0f;

        float totalWeight = 0f;

        for (int i = 0; i < patterns.Length; i++)
        {
            SpawnPattern pattern = patterns[i];

            if (pattern == null)
                continue;

            if (currentDistance < pattern.unlockDistance)
                continue;

            totalWeight += Mathf.Max(0f, pattern.weight);
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < patterns.Length; i++)
        {
            SpawnPattern pattern = patterns[i];

            if (pattern == null)
                continue;

            if (currentDistance < pattern.unlockDistance)
                continue;

            currentWeight += Mathf.Max(0f, pattern.weight);

            if (randomValue <= currentWeight)
                return pattern;
        }

        return null;
    }

    private void Spawn(SpawnPattern pattern)
    {
        if (pattern.elements == null)
            return;

        for (int i = 0; i < pattern.elements.Length; i++)
        {
            PatternSpawnElement element = pattern.elements[i];

            if (element == null || element.prefab == null)
                continue;

            float laneX = GetLaneX(element.laneIndex);

            Vector3 spawnPosition = new Vector3(
                laneX,
                element.y,
                spawnZ + element.zOffset
            );

            Instantiate(element.prefab, spawnPosition, Quaternion.identity);
        }

        if (showDebugLog)
        {
            Debug.Log($"[PatternSpawner] Spawned Pattern: {pattern.patternName}");
        }
    }

    private void ResetNextSpawnDistance(SpawnPattern pattern)
    {
        float min = Mathf.Max(1f, pattern.minNextDistance);
        float max = Mathf.Max(min, pattern.maxNextDistance);

        distanceUntilNextSpawn = Random.Range(min, max);
    }

    private float GetLaneX(int laneIndex)
    {
        if (laneXPositions == null || laneXPositions.Length == 0)
            return 0f;

        int clampedIndex = Mathf.Clamp(laneIndex, 0, laneXPositions.Length - 1);

        return laneXPositions[clampedIndex];
    }
}