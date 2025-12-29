using System.Collections.Generic;

namespace TurnBase
{
    public struct PlayerRotationResult
    {
        public bool IsNewTurn;
        public List<IPlayer> PlayersInTurn;
    }
}