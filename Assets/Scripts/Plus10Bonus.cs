using UnityEngine;

public class Plus10Bonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "+10";
        backgroundColor = Color.blue;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.AddBalls(10, true);
        Destroy(gameObject);
    }
}
