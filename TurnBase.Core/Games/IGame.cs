namespace TurnBase.Core;

public interface IGame
{
  void addPlayer(IPlayer player);
  IReadOnlyCollection<IPlayer> getPlayers();

  Task initPlayer(int playerNumber);
  Task gameProcess();
}
