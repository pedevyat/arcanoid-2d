using UnityEngine;
using UnityEngine.UI;

public class BonusBase : MonoBehaviour
{
    public string bonusText = "+100";
    public Color backgroundColor = Color.yellow;
    public Color textColor = Color.black;

    void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = backgroundColor;

        var textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = bonusText;
            textComponent.color = textColor;
        }
    }

    public virtual void BonusActivate()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.GetComponent<PlayerScript>().BlockDestroyed(100);
        Destroy(gameObject);
    }
}
