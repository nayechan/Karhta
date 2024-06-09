namespace InGame.UI
{
    public class HealthBar : Bar
    {
        public override void OnPlayerRefresh(Player.Player player)
        {
            Refresh(player.CurrentHp, player.MaxHp);
        }
    }
}
