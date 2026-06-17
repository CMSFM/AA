using UnityEngine;

public class GroundLoopController : MonoBehaviour
{
    [Header("Ground Segments")]
    [SerializeField] private Transform[] segments;
    [SerializeField] private float segmentLength = 30f;
    [SerializeField] private float recycleBehindZ = -30f;

    private void Update()
    {
        float speed = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.CurrentSpeed
            : 0f;

        if (speed <= 0f)
            return;

        MoveSegments(speed);
        RecycleSegments();
    }

    private void MoveSegments(float speed)
    {
        Vector3 movement = Vector3.back * speed * Time.deltaTime;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null)
                continue;

            segments[i].position += movement;
        }
    }

    private void RecycleSegments()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            Transform segment = segments[i];

            if (segment == null)
                continue;

            if (segment.position.z > recycleBehindZ)
                continue;

            float frontZ = GetFrontMostZ();

            Vector3 position = segment.position;
            position.z = frontZ + segmentLength;
            segment.position = position;
        }
    }

    private float GetFrontMostZ()
    {
        float frontZ = float.MinValue;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null)
                continue;

            if (segments[i].position.z > frontZ)
                frontZ = segments[i].position.z;
        }

        return frontZ;
    }
}