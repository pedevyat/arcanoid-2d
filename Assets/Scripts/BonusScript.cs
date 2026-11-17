using UnityEngine;

public class BonusScript : MonoBehaviour
{
    public float speed = 2f;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.down * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var bonus = GetComponent<BonusBase>();
            if (bonus != null)
                bonus.BonusActivate();
            else
                Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") && other.gameObject.name == "Bottom Wall")
        {
            Destroy(gameObject);
        }
    }
}
