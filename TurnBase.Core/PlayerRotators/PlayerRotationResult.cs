using System.Collections.Generic;

namespace TurnBase.Core
{
    public struct PlayerRotationResult
    {
        public bool IsNewTurn;
        public List<IPlayer> PlayersInTurn;
    }
}