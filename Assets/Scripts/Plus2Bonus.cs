using UnityEngine;

public class Plus2Bonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "+2";
        backgroundColor = Color.blue;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.AddBalls(2, true);
        Destroy(gameObject);
    }
}
