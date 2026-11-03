using System;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class AnyPlayerGameOverCondition : IGameOverCondition
    {
        public AnyPlayerGameOverCondition(IGameCondition gameCondition)
        {
            this.GameCondition = gameCondition ?? throw new ArgumentNullException(nameof(gameCondition));
        }

        public IGameCondition GameCondition { get; }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            for (var playerId = 0; playerId < rules.GetMaxPlayersCount(); playerId++)
            {
                if (this.GameCondition.CheckCondition(mainField, rules, playerId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}