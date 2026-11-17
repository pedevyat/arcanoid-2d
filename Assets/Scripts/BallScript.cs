using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public Vector2 ballInitialForce;
    Rigidbody2D rb;
    GameObject playerObj; 
    float deltaX;
    AudioSource audioSrc; 
    public AudioClip hitSound; 
    public AudioClip loseSound;
    public GameDataScript gameData;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerObj = GameObject.FindGameObjectWithTag("Player"); 
        deltaX = transform.position.x;
        audioSrc = Camera.main.GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        if (rb.isKinematic) 
            if (Input.GetButtonDown("Fire1")) 
                Launch();
            else 
            { 
                var pos = transform.position;
                pos.x = playerObj.transform.position.x + deltaX; 
                transform.position = pos; 
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
        var rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.isKinematic = false;
        rigidbody2D.AddForce(ballInitialForce);
    }

    public void ChangeSpeed(float multiplier)
    {
        var rigidbody2D = GetComponent<Rigidbody2D>();
        if (rigidbody2D.isKinematic)
            ballInitialForce *= multiplier;
        else
            rigidbody2D.velocity *= multiplier;
    }

    void OnTriggerEnter2D(Collider2D other) 
    { 
        Destroy(gameObject);
        audioSrc.PlayOneShot(loseSound);
        playerObj.GetComponent<PlayerScript>().BallDestroyed();
    }

    void OnCollisionEnter2D(Collision2D collision) 
    {
        audioSrc.PlayOneShot(hitSound); 
    }
}
