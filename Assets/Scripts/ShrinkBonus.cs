using UnityEngine;

public class ShrinkBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Shrink";
        backgroundColor = Color.red;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.Shrink();
        Destroy(gameObject);
    }
}
