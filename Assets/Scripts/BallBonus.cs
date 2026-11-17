using UnityEngine;

public class BallBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Ball";
        backgroundColor = Color.green;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.AddBalls(1, false);
        Destroy(gameObject);
    }
}
