using UnityEngine;

public class GunBonus : BonusBase
{
    static readonly Color LightPurple = new Color(0.75f, 0.6f, 1f, 1f);

    protected override void Configure()
    {
        bonusText = "Gun";
        backgroundColor = LightPurple;
        textColor = Color.white;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.EquipGun(false);
        Destroy(gameObject);
    }
}
