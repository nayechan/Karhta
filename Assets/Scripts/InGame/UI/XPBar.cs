using System;
using InGame.Player;
using InGame.UI;

public class XPBar : Bar
{
    public override void OnPlayerRefresh(Player player)
    {
        Refresh(player.currentXP, player.MaxXp);
    }
}
