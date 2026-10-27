using System;
using System.Collections.Generic;
using System.Linq;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class OrVictoryCondition : IVictoryCondition
    {
        public OrVictoryCondition(params IVictoryCondition[] conditions)
        {
            this.Conditions = conditions?.ToList() ?? throw new ArgumentNullException(nameof(conditions));
        }

        public List<IVictoryCondition> Conditions { get; }

        public bool IsWinner(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return this.Conditions.Any(condition => condition.IsWinner(mainField, rules, playerNumber));
        }
    }
}