namespace BattleshipMelee.Server.Models;

public record PlayerStatus(PlayerId PlayerId)
{
	public bool IsPlaying { get; set; }
	public bool IsWaiting { get; set; }
}
