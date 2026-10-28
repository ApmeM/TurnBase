using System;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class AnyPlayerVictoryGameOverCondition : IGameOverCondition
    {
        public AnyPlayerVictoryGameOverCondition(IVictoryCondition victoryCondition)
        {
            this.VictoryCondition = victoryCondition ?? throw new ArgumentNullException(nameof(victoryCondition));
        }

        public IVictoryCondition VictoryCondition { get; }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            for (var playerId = 0; playerId < rules.GetMaxPlayersCount(); playerId++)
            {
                if (this.VictoryCondition.IsWinner(mainField, rules, playerId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}