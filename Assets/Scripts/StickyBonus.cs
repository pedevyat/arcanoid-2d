using UnityEngine;

public class StickyBonus : BonusBase
{
    static readonly Color LightGreen = new Color(0.6f, 1f, 0.6f, 1f);

    protected override void Configure()
    {
        bonusText = "Sticky";
        backgroundColor = LightGreen;
        textColor = Color.black;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.SetSticky(true);
        Destroy(gameObject);
    }
}
