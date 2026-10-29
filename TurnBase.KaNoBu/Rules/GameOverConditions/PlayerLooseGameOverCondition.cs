using System;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class PlayerLooseGameOverCondition : IGameOverCondition
    {
        public PlayerLooseGameOverCondition(int playerId, ILooseCondition looseCondition)
        {
            this.PlayerId = playerId;
            this.LooseCondition = looseCondition ?? throw new ArgumentNullException(nameof(looseCondition));
        }

        public int PlayerId { get; }

        public ILooseCondition LooseCondition { get; }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            return this.LooseCondition.IsLooser(mainField, rules, this.PlayerId);
        }
    }
}