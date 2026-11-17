using UnityEngine;

public class MovingBlockScript : MonoBehaviour
{
    public float speed = 2f;
    public float lookAhead = 0.05f;

    int direction;
    Collider2D blockCollider;
    int obstacleMask;

    void Start()
    {
        blockCollider = GetComponent<Collider2D>();
        obstacleMask = LayerMask.GetMask("Block", "Wall");
        direction = Random.value < 0.5f ? -1 : 1;
    }

    void FixedUpdate()
    {
        if (ObstacleAhead())
            direction = -direction;

        transform.position += Vector3.right * direction * speed * Time.fixedDeltaTime;
        Physics2D.SyncTransforms();
    }

    bool ObstacleAhead()
    {
        if (blockCollider == null)
            return false;

        var hits = Physics2D.BoxCastAll(
            transform.position,
            blockCollider.bounds.size * 0.9f,
            0f,
            Vector2.right * direction,
            lookAhead,
            obstacleMask);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != gameObject)
                return true;
        }

        return false;
    }
}
