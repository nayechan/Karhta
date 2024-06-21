using InGame.Player;
using TMPro;
using UnityEngine;

namespace InGame.UI.Pages
{
    public class StatusDistributionPage : MenuPage, IPlayerListener
    {
        [SerializeField] private TMP_Text apText, atkText, defText, hpText;
        [SerializeField] private Player.Player player;
        public void OnPlayerRefresh(Player.Player _player)
        {
            apText.text = GetDescText(_player, "AP");
            atkText.text = GetDescText(_player, "ATK");
            defText.text = GetDescText(_player, "DEF");
            hpText.text = GetDescText(_player, "HP");
        }

        public string GetDescText(Player.Player _player, string statType)
        {
            var value = _player.PlayerStatData.GetStat(statType.ToLower());
            return $"{statType} : {value:0.0}";
        }

        public void DistributeStat(string statType)
        {
            player.PlayerStatData.AddStat(statType.ToLower());
            player.Refresh();
        }
    }
}
