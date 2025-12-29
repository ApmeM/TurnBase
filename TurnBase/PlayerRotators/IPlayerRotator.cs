using System.Collections.Generic;

namespace TurnBase
{
    public interface IPlayerRotator
    {
        PlayerRotationResult MoveNext(List<IPlayer> current, List<IPlayer> allPlayers);
    }
}