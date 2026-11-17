using UnityEngine;

public class MovingBlueBlockScript : MonoBehaviour
{
    public Vector3 offsetA = new Vector3(-2f, 0f, 0f);
    public Vector3 offsetB = new Vector3(2f, 0f, 0f);
    public float speed = 1.5f;

    Vector3 pointA;
    Vector3 pointB;
    Vector3 target;

    void Start()
    {
        pointA = transform.position + offsetA;
        pointB = transform.position + offsetB;
        target = pointB;
    }

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);

        if (transform.position == target)
            target = target == pointB ? pointA : pointB;

        Physics2D.SyncTransforms();
    }
}
