using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockScript : MonoBehaviour
{
    public GameObject textObject; 
    protected Text textComponent;
    public int hitsToDestroy; 
    public int points;
    public bool countsForLevelCompletion = true;
    public GameObject bonusPrefab;
    protected PlayerScript playerScript;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (textObject != null)
        {
            textComponent = textObject.GetComponent<Text>();
            RefreshLabel();
        }
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    protected virtual string GetLabel()
    {
        return hitsToDestroy.ToString();
    }

    protected void RefreshLabel()
    {
        if (textComponent != null)
            textComponent.text = GetLabel();
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) 
    { 
        var ball = collision.gameObject.GetComponent<BallScript>();
        if (ball == null)
            return;

        HandleBallHit(ball);
    }

    protected virtual void HandleBallHit(BallScript ball)
    {
        ApplyHit(ball.damage);
    }

    public virtual void ApplyHit(int damage)
    {
        hitsToDestroy -= damage;

        if (hitsToDestroy <= 0)
            DestroyBlock();
        else
            RefreshLabel();
    }

    protected virtual void DestroyBlock()
    {
        var position = transform.position;
        Destroy(gameObject);
        if (bonusPrefab != null)
            playerScript.CreateBonus(position, bonusPrefab);
        playerScript.BlockDestroyed(points);
    }
}
