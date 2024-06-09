using InGame.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public abstract class Bar : MonoBehaviour, IPlayerListener
    {
        [SerializeField] protected TMP_Text text;
        [SerializeField] protected Image foreground;

        protected virtual void Refresh(float amount, float maxAmount)
        {
            text.text = string.Format("{0:N0} / {1:N0}", amount, maxAmount);
            foreground.fillAmount = amount / maxAmount;
        }

        public abstract void OnPlayerRefresh(Player.Player player);
    }
}
