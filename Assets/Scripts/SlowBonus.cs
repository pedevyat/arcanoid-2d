using UnityEngine;

public class SlowBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Slow";
        backgroundColor = Color.green;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.ChangeBallsSpeed(0.9f);
        Destroy(gameObject);
    }
}
