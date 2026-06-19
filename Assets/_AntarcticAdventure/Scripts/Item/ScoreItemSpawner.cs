using UnityEngine;

public class ScoreItemSpawner : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Spawn Position")]
    [SerializeField] private float spawnZ = 35f;
    [SerializeField] private float spawnY = 1f;
    [SerializeField] private float[] laneXPositions = new float[] { -3f, 0f, 3f };

    [Header("Item Line")]
    [SerializeField] private int itemsPerLine = 3;
    [SerializeField] private float itemSpacingZ = 2f;

    [Header("Spawn Distance")]
    [SerializeField] private float initialSpawnDistance = 8f;
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private float maxSpawnDistance = 18f;

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
            SpawnItemLine();
            ResetSpawnDistance();
        }
    }

    private void SpawnItemLine()
    {
        if (itemPrefab == null)
            return;

        float laneX = PickLaneX();

        for (int i = 0; i < itemsPerLine; i++)
        {
            Vector3 position = new Vector3(
                laneX,
                spawnY,
                spawnZ + itemSpacingZ * i
            );

            Instantiate(itemPrefab, position, Quaternion.identity);
        }
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
}