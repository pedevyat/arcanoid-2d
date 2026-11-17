using UnityEngine;

public class NormBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Norm";
        backgroundColor = Color.white;
        textColor = Color.black;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.SetBallsDamage(1, Color.white);
        Destroy(gameObject);
    }
}
