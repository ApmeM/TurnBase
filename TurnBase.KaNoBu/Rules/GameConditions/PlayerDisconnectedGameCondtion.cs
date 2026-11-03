using TurnBase.KaNoBu;

namespace TurnBase
{
    public class PlayerDisconnectedGameCondition : IGameCondition
    {
        public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return mainField.DisconnectedPlayers.Contains(playerNumber);
        }
    }
}
