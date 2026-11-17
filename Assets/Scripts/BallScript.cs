using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public Vector2 ballInitialForce;
    public int damage = 1;
    Rigidbody2D rb;
    GameObject playerObj; 
    float deltaX;
    bool isStuck;
    AudioSource audioSrc; 
    public AudioClip hitSound; 
    public AudioClip loseSound;
    public GameDataScript gameData;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerObj = GameObject.FindGameObjectWithTag("Player"); 
        if (!isStuck)
            deltaX = transform.position.x;
        audioSrc = Camera.main.GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        if (rb.isKinematic) 
        {
            if (isStuck && playerObj != null)
            {
                var playerCollider = playerObj.GetComponent<Collider2D>();
                var ballCollider = GetComponent<Collider2D>();
                if (playerCollider != null && ballCollider != null)
                {
                    var stuckPosition = transform.position;
                    stuckPosition.y = playerCollider.bounds.max.y + ballCollider.bounds.extents.y;
                    transform.position = stuckPosition;
                }
            }

            if (Input.GetButtonDown("Fire1")) 
                Launch();
            else 
            { 
                var pos = transform.position;
                pos.x = playerObj.transform.position.x + deltaX; 
                transform.position = pos; 
            }
        }
        if (!rb.isKinematic && Input.GetKeyDown(KeyCode.J)) 
        { 
            var v = rb.velocity; 
            if (Random.Range(0, 2) == 0) 
                v.Set(v.x - 0.1f, v.y + 0.1f); 
            else 
                v.Set(v.x + 0.1f, v.y - 0.1f); 
            rb.velocity = v;
        }
    }

    public void Launch()
    {
        if (isStuck)
        {
            isStuck = false;
            transform.position += Vector3.up * 0.1f;
            if (playerObj != null)
            {
                var playerScript = playerObj.GetComponent<PlayerScript>();
                if (playerScript != null)
                    playerScript.BallLaunched(this);
            }
        }

        var rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.isKinematic = false;
        rigidbody2D.AddForce(ballInitialForce);
    }

    public void StickToPlayer()
    {
        if (playerObj == null)
            playerObj = GameObject.FindGameObjectWithTag("Player");

        var rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.angularVelocity = 0f;
        rigidbody2D.isKinematic = true;

        var ballCollider = GetComponent<Collider2D>();
        var playerCollider = playerObj.GetComponent<Collider2D>();
        if (ballCollider != null && playerCollider != null)
        {
            var position = transform.position;
            position.y = playerCollider.bounds.max.y + ballCollider.bounds.extents.y;
            transform.position = position;
        }

        deltaX = transform.position.x - playerObj.transform.position.x;
        isStuck = true;
    }

    public void ReleaseFromStick()
    {
        if (isStuck)
            Launch();
    }

    public void ChangeSpeed(float multiplier)
    {
        var rigidbody2D = GetComponent<Rigidbody2D>();
        if (rigidbody2D.isKinematic)
            ballInitialForce *= multiplier;
        else
            rigidbody2D.velocity *= multiplier;
    }

    public void SetDamage(int newDamage, Color color)
    {
        damage = newDamage;
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    void OnTriggerEnter2D(Collider2D other) 
    { 
        Destroy(gameObject);
        audioSrc.PlayOneShot(loseSound);
        playerObj.GetComponent<PlayerScript>().BallDestroyed();
    }

    void OnCollisionEnter2D(Collision2D collision) 
    {
        if (audioSrc != null)
            audioSrc.PlayOneShot(hitSound); 

        if (collision.gameObject.CompareTag("Player"))
        {
            var playerScript = collision.gameObject.GetComponent<PlayerScript>();
            if (playerScript != null)
                playerScript.TryStickBall(this);
        }
    }
}
