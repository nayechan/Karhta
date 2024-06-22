using InGame.Player;
using TMPro;
using UnityEngine;

namespace InGame.UI.Pages
{
    public class StatusInformationPage : MenuPage, IPlayerListener
    {
        [SerializeField] private RectTransform statusInformationContentTransform;
        [SerializeField] private TMP_Text statusInformationText;
        
        public void OnPlayerRefresh(Player.Player _player)
        {
            var playerStat = _player.PlayerStatData;
            var playerWeapon = _player.GetCurrentWeapon();

            var allStatMultiplier = _player.allStatMultiplier;
            var xpMultiplier = _player.xpMultiplier;

            var atkStat = Mathf.RoundToInt(playerStat.StatAtk * allStatMultiplier);
            var weaponDamagePoint = playerWeapon != null ? playerWeapon.GetDamagePoint() : 0;
            var finalDamage = atkStat + weaponDamagePoint + 1;

            var defStat = Mathf.RoundToInt(playerStat.StatDef * allStatMultiplier);
            var minDamageReduction = defStat / 2;
            var maxDamageReduction = defStat;

            var baseHp = _player.currentLevel * 17 + 4;
            var hpStat = Mathf.RoundToInt(playerStat.StatHp * allStatMultiplier);
            var finalHp = baseHp + hpStat;

            var content =
                "Status Information\n\n" +
                $"Global All Stat Multiplier : x{allStatMultiplier:0.00}\n" +
                $"XP Multiplier : x{xpMultiplier:0.00}\n" +
                $"ATK Stat : {atkStat:N0}\n" +
                $"Weapon Damage Point : {weaponDamagePoint:N0}\n" +
                $"Final Damage : {finalDamage}\n\n" +
                $"DEF Stat : {defStat:N0}\n" +
                $"Final Damage Reduction : {minDamageReduction:N0} ~ {maxDamageReduction:N0}\n\n" +
                $"Base HP : {baseHp:N0}\n" +
                $"HP Stat : {hpStat:N0}\n" +
                $"Final HP : {finalHp:N0}\n\n";

            statusInformationText.text = content;

            var contentSize = statusInformationContentTransform.sizeDelta;
            contentSize.y = statusInformationText.bounds.size.y;
            statusInformationContentTransform.sizeDelta = contentSize;
        }
    }
}