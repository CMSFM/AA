using UnityEngine;

public class WorldMover : MonoBehaviour
{
    [Header("Destroy")]
    [SerializeField] private bool destroyWhenBehind = true;
    [SerializeField] private float destroyZ = -15f;

    private void Update()
    {
        float speed = WorldScrollManager.Instance != null
            ? WorldScrollManager.Instance.CurrentSpeed
            : 0f;

        if (speed <= 0f)
            return;

        transform.position += Vector3.back * speed * Time.deltaTime;

        if (destroyWhenBehind && transform.position.z <= destroyZ)
        {
            Destroy(gameObject);
        }
    }
}