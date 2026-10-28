using TurnBase.KaNoBu;

namespace TurnBase
{
    public class AllButOnePlayerLostGameOverCondition : IGameOverCondition
    {
        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            var inGamePlayers = 0;
            for (var playerNumber = 0; playerNumber < rules.GetMaxPlayersCount(); playerNumber++)
            {
                if (!mainField.LostPlayers.Contains(playerNumber))
                {
                    inGamePlayers++;
                }
            }

            return inGamePlayers == 1;
        }
    }
}