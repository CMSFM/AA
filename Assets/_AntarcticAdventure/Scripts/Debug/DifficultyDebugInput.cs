using UnityEngine;

public class DifficultyDebugInput : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugKeys = false;

    [Header("Jump To Distance")]
    [SerializeField] private float easyDistance = 0f;
    [SerializeField] private float normalDistance = 100f;
    [SerializeField] private float hardDistance = 250f;
    [SerializeField] private float extremeDistance = 500f;

    [Header("Add Distance")]
    [SerializeField] private float addDistanceAmount = 50f;

    private void Update()
    {
        if (!enableDebugKeys)
            return;

        if (WorldScrollManager.Instance == null)
            return;

        if (Input.GetKeyDown(KeyCode.F1))
            SetDistance(easyDistance);

        if (Input.GetKeyDown(KeyCode.F2))
            SetDistance(normalDistance);

        if (Input.GetKeyDown(KeyCode.F3))
            SetDistance(hardDistance);

        if (Input.GetKeyDown(KeyCode.F4))
            SetDistance(extremeDistance);

        if (Input.GetKeyDown(KeyCode.PageUp))
            AddDistance(addDistanceAmount);
    }

    private void SetDistance(float distance)
    {
        WorldScrollManager.Instance.SetDistanceForDebug(distance);

        Debug.Log($"[DifficultyDebug] Distance set to {distance:0}m");
    }

    private void AddDistance(float amount)
    {
        float currentDistance = WorldScrollManager.Instance.Distance;
        float nextDistance = currentDistance + amount;

        WorldScrollManager.Instance.SetDistanceForDebug(nextDistance);

        Debug.Log($"[DifficultyDebug] Distance added. Current: {nextDistance:0}m");
    }
}