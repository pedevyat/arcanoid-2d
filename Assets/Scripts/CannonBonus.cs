using UnityEngine;

public class CannonBonus : BonusBase
{
    static readonly Color DarkPurple = new Color(0.35f, 0.12f, 0.6f, 1f);

    protected override void Configure()
    {
        bonusText = "Cannon";
        backgroundColor = DarkPurple;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.EquipGun(true);
        Destroy(gameObject);
    }
}
