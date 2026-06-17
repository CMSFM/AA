using UnityEngine;

[System.Serializable]
public class ObstacleSpawnDefinition
{
    public GameObject prefab;
    public float spawnY = 0.4f;
    public float weight = 1f;
}

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Prefabs")]
    [SerializeField] private ObstacleSpawnDefinition[] obstacleDefinitions;

    [Header("Spawn Position")]
    [SerializeField] private float spawnZ = 35f;
    [SerializeField] private float[] laneXPositions = new float[] { -3f, 0f, 3f };

    [Header("Spawn Distance")]
    [SerializeField] private float initialSpawnDistance = 12f;
    [SerializeField] private float minSpawnDistance = 9f;
    [SerializeField] private float maxSpawnDistance = 16f;

    private float distanceUntilNextSpawn;

    private void Start()
    {
        distanceUntilNextSpawn = initialSpawnDistance;
    }

    private void Update()
    {
        if (AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsGameOver)
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
            SpawnObstacle();
            ResetSpawnDistance();
        }
    }

    private void SpawnObstacle()
    {
        ObstacleSpawnDefinition definition = PickObstacleDefinition();

        if (definition == null || definition.prefab == null)
            return;

        float laneX = PickLaneX();

        Vector3 spawnPosition = new Vector3(
            laneX,
            definition.spawnY,
            spawnZ
        );

        Instantiate(definition.prefab, spawnPosition, Quaternion.identity);
    }

    private void ResetSpawnDistance()
    {
        distanceUntilNextSpawn = Random.Range(minSpawnDistance, maxSpawnDistance);
    }

    private float PickLaneX()
    {
        if (laneXPositions == null || laneXPositions.Length == 0)
            return 0f;

        int index = Random.Range(0, laneXPositions.Length);
        return laneXPositions[index];
    }

    private ObstacleSpawnDefinition PickObstacleDefinition()
    {
        if (obstacleDefinitions == null || obstacleDefinitions.Length == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < obstacleDefinitions.Length; i++)
        {
            totalWeight += Mathf.Max(0f, obstacleDefinitions[i].weight);
        }

        if (totalWeight <= 0f)
            return obstacleDefinitions[0];

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < obstacleDefinitions.Length; i++)
        {
            currentWeight += Mathf.Max(0f, obstacleDefinitions[i].weight);

            if (randomValue <= currentWeight)
                return obstacleDefinitions[i];
        }

        return obstacleDefinitions[obstacleDefinitions.Length - 1];
    }
}