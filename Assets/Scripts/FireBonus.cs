using UnityEngine;

public class FireBonus : BonusBase
{
    static readonly Color Orange = new Color(1f, 0.5f, 0f, 1f);

    protected override void Configure()
    {
        bonusText = "Fire";
        backgroundColor = Orange;
        textColor = Color.black;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.SetBallsDamage(4, Orange);
        Destroy(gameObject);
    }
}
