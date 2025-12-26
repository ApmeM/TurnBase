
namespace TurnBase.Core;

public class PlayerRotatorNormal : IPlayerRotator
{
    public IPlayerRotator.PlayerRotationResult MoveNext(List<IPlayer>? current, List<IPlayer> allPlayers)
    {
        if (allPlayers.Count == 0)
        {
            return new IPlayerRotator.PlayerRotationResult
            {
                IsNewTurn = true,
                PlayersInTurn = new List<IPlayer>()
            };
        }

        if (current == null || current.Count == 0)
        {
            return new IPlayerRotator.PlayerRotationResult
            {
                IsNewTurn = true,
                PlayersInTurn = new List<IPlayer> { allPlayers[0] }
            };
        }

        if (current.Count != 1)
        {
            throw new Exception($"Current players list must contain exactly 0 or 1 player for {nameof(PlayerRotatorNormal)}.");
        }

        var currentIndex = allPlayers.IndexOf(current[0]);
        var nextIndex = (currentIndex + 1) % allPlayers.Count;
        var isNewTurn = nextIndex == 0;

        return new IPlayerRotator.PlayerRotationResult
        {
            IsNewTurn = isNewTurn,
            PlayersInTurn = new List<IPlayer> { allPlayers[nextIndex] }
        };
    }
}