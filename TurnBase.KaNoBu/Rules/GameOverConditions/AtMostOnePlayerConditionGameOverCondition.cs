using System;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class AtMostOnePlayerGameOverCondition : IGameOverCondition
    {
        public AtMostOnePlayerGameOverCondition(IGameCondition playerCondition)
        {
            this.PlayerCondition = playerCondition ?? throw new ArgumentNullException(nameof(playerCondition));
        }

        public IGameCondition PlayerCondition { get; }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            var playersMatchingCondition = 0;
            for (var playerNumber = 0; playerNumber < rules.GetMaxPlayersCount(); playerNumber++)
            {
                if (this.PlayerCondition.CheckCondition(mainField, rules, playerNumber))
                {
                    playersMatchingCondition++;
                }
            }

            return playersMatchingCondition <= 1;
        }
    }
}