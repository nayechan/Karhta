using InGame.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class LevelIndicator : MonoBehaviour, IPlayerListener
    {
        [SerializeField] protected TMP_Text text;

        public void Refresh(long amount)
        {
            text.text = string.Format("Lv. {0:N0}", amount);
        }

        public void OnPlayerRefresh(Player.Player player)
        {
            Refresh(player.currentLevel);
        }
    }
}