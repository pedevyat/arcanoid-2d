using UnityEngine;

public class SimpleBonus : BonusBase
{
    protected override void Configure()
    {
        bonusText = "Simple";
        backgroundColor = Color.white;
        textColor = Color.black;
    }

    public override void BonusActivate()
    {
        var playerScript = GetPlayerScript();
        if (playerScript != null)
            playerScript.SetSticky(false);
        Destroy(gameObject);
    }
}
