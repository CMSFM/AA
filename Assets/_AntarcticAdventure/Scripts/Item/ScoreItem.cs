using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScoreItem : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private int scoreAmount = 100;

    [Header("Visual")]
    [SerializeField] private float rotateSpeed = 180f;

    private bool collected;

    private void Reset()
    {
        Collider itemCollider = GetComponent<Collider>();

        if (itemCollider != null)
            itemCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider itemCollider = GetComponent<Collider>();

        if (itemCollider != null)
            itemCollider.isTrigger = true;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        AntarcticPlayerController player =
            other.GetComponentInParent<AntarcticPlayerController>();

        if (player == null)
            return;

        Collect();
    }

    private void Collect()
    {
        collected = true;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddItemScore(scoreAmount);

        Destroy(gameObject);
    }
}