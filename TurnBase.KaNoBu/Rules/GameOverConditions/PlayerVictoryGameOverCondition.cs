using System;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class PlayerVictoryGameOverCondition : IGameOverCondition
    {
        public PlayerVictoryGameOverCondition(int playerId, IVictoryCondition victoryCondition)
        {
            this.PlayerId = playerId;
            this.VictoryCondition = victoryCondition ?? throw new ArgumentNullException(nameof(victoryCondition));
        }

        public int PlayerId { get; }

        public IVictoryCondition VictoryCondition { get; }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            return this.VictoryCondition.IsWinner(mainField, rules, this.PlayerId);
        }
    }
}