using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockScript : MonoBehaviour
{
    public GameObject textObject; 
    Text textComponent;  // TMP_Text textComponent; 
    public int hitsToDestroy; 
    public int points;
    public bool countsForLevelCompletion = true;
    public GameObject bonusPrefab;
    PlayerScript playerScript;

    // Start is called before the first frame update
    void Start()
    {
        if (textObject != null)
        {
            textComponent = textObject.GetComponent<Text>();
            textComponent.text = hitsToDestroy.ToString();
        }
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    void OnCollisionEnter2D(Collision2D collision) 
    { 
        { 
            var ball = collision.gameObject.GetComponent<BallScript>();
            if (ball == null)
                return;

            hitsToDestroy -= ball.damage;
            if (hitsToDestroy <= 0)
            {
                print(points);
                var position = transform.position;
                Destroy(gameObject);
                if (bonusPrefab != null)
                    playerScript.CreateBonus(position, bonusPrefab);
                playerScript.BlockDestroyed(points);
            }
            else if (textComponent != null)
                textComponent.text = hitsToDestroy.ToString();
        } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
