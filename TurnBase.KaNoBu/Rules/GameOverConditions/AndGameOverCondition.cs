using System;
using System.Collections.Generic;
using System.Linq;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class AndGameOverCondition: IGameOverCondition
    {
        public AndGameOverCondition(params IGameOverCondition[] conditions)
        {
            this.Conditions = conditions?.ToList() ?? throw new ArgumentNullException(nameof(conditions));
        }

        public List<IGameOverCondition> Conditions { get; }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            return this.Conditions.All(condition => condition.IsGameOver(mainField, rules));
        }
    }
}