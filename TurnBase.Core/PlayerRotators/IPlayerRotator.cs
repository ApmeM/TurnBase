using System.Collections.Generic;

namespace TurnBase.Core
{
    public interface IPlayerRotator
    {
        PlayerRotationResult MoveNext(List<IPlayer> current, List<IPlayer> allPlayers);
    }
}