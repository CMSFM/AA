using UnityEngine;

public enum ObstacleAvoidType
{
    Jump,
    Slide
}

[RequireComponent(typeof(Collider))]
public class ObstacleHitbox : MonoBehaviour
{
    [SerializeField] private ObstacleAvoidType avoidType;

    private Collider hitboxCollider;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
    }

    private void Reset()
    {
        Collider collider = GetComponent<Collider>();

        if (collider != null)
            collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        AntarcticPlayerController player =
            other.GetComponentInParent<AntarcticPlayerController>();

        if (player == null)
            return;

        if (AntarcticGameManager.Instance != null &&
            AntarcticGameManager.Instance.IsGameOver)
        {
            return;
        }

        if (IsAvoided(player))
            return;

        if (AntarcticGameManager.Instance != null)
            AntarcticGameManager.Instance.GameOver();
    }

    private bool IsAvoided(AntarcticPlayerController player)
    {
        switch (avoidType)
        {
            case ObstacleAvoidType.Slide:
                return player.IsSliding;

            case ObstacleAvoidType.Jump:
                return false;

            default:
                return false;
        }
    }
}