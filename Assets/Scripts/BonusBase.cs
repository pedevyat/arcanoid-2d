using UnityEngine;
using UnityEngine.UI;

public class BonusBase : MonoBehaviour
{
    public string bonusText = "+100";
    public Color backgroundColor = Color.yellow;
    public Color textColor = Color.black;

    protected virtual void Configure()
    {
    }

    void Awake()
    {
        RefreshAppearance();
    }

    public void RefreshAppearance()
    {
        Configure();
        ApplyAppearance();
    }

    protected void ApplyAppearance()
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

    protected PlayerScript GetPlayerScript()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.GetComponent<PlayerScript>() : null;
    }

    public virtual void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.BlockDestroyed(100);
        Destroy(gameObject);
    }
}
