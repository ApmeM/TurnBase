using System.Collections.Generic;

namespace TurnBase
{
    public class PlayerRotatorNoRotation : IPlayerRotator
    {
        public PlayerRotationResult MoveNext(List<IPlayer> current, List<IPlayer> allPlayers)
        {
            return new PlayerRotationResult
            {
                IsNewTurn = true,
                PlayersInTurn = new List<IPlayer>()
            };
        }
    }
}