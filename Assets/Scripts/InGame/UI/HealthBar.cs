namespace InGame.UI
{
    public class HealthBar : Bar
    {
        public override void OnPlayerRefresh(Player.Player _player)
        {
            Refresh(_player.CurrentHp, _player.MaxHp);
        }
    }
}
