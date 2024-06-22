using System;
using InGame.Player;
using InGame.UI;

public class XPBar : Bar
{
    public override void OnPlayerRefresh(Player _player)
    {
        Refresh(_player.CurrentXP, _player.MaxXp);
    }
}
