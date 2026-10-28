using TurnBase.KaNoBu;

namespace TurnBase
{
    public class AllPlayersLostGameOverCondition : IGameOverCondition
    {
        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            for (var playerNumber = 0; playerNumber < rules.GetMaxPlayersCount(); playerNumber++)
            {
                if (!mainField.LostPlayers.Contains(playerNumber))
                {
                    return false;
                }
            }

            return true;
        }
    }
}