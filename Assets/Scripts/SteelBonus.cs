using UnityEngine;

public class SteelBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Steel";
        backgroundColor = Color.gray;
        textColor = Color.black;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.SetBallsDamage(40, Color.gray);
        Destroy(gameObject);
    }
}
