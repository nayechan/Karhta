using System;

namespace InGame.Player
{
    public interface IPlayerListener
    {
        void OnPlayerRefresh(Player player);
    }
}