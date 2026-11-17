using UnityEngine;

public class BombBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Bomb";
        backgroundColor = Color.black;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.DetonateBomb();
        Destroy(gameObject);
    }
}
