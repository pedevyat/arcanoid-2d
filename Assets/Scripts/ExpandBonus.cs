using UnityEngine;

public class ExpandBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Expand";
        backgroundColor = Color.green;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.Expand();
        Destroy(gameObject);
    }
}
