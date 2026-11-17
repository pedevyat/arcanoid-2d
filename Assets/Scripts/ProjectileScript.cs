using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public int damage = 1;
    public float speed = 14f;
    public Vector2 direction = Vector2.up;

    Color effectColor = new Color(0.8f, 0.4f, 1f, 1f);
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Launch();
    }

    public void Initialize(int newDamage, Color color)
    {
        damage = newDamage;
        effectColor = color;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = color;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        Launch();
    }

    void Launch()
    {
        if (rb != null)
            rb.velocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var block = other.GetComponent<BlockScript>();
        if (block != null)
        {
            block.ApplyHit(damage);
            HitEffects.ProjectileBlock(transform.position, effectColor);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            HitEffects.Play(transform.position, effectColor, 8, 3f);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (transform.position.y > 6f || Mathf.Abs(transform.position.x) > 10f)
            Destroy(gameObject);
    }
}
