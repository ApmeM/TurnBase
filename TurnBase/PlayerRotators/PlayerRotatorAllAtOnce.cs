
using System.Collections.Generic;

namespace TurnBase
{
    public class PlayerRotatorAllAtOnce : IPlayerRotator
    {
        public PlayerRotationResult MoveNext(List<IPlayer> current, List<IPlayer> allPlayers)
        {
            return new PlayerRotationResult
            {
                IsNewTurn = true,
                PlayersInTurn = allPlayers
            };
        }
    }
}