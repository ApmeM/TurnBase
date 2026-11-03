using System;
using System.Collections.Generic;
using System.Linq;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class AndGameCondition : IGameCondition
    {
        public AndGameCondition(params IGameCondition[] conditions)
        {
            this.Conditions = conditions?.ToList() ?? throw new ArgumentNullException(nameof(conditions));
        }

        public List<IGameCondition> Conditions { get; }

        public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return this.Conditions.All(condition => condition.CheckCondition(mainField, rules, playerNumber));
        }
    }
}