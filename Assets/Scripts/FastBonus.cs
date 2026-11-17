using UnityEngine;

public class FastBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Fast";
        backgroundColor = Color.red;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.ChangeBallsSpeed(1.1f);
        Destroy(gameObject);
    }
}
