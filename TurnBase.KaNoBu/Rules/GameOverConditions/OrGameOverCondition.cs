using System;
using System.Collections.Generic;
using System.Linq;
using TurnBase.KaNoBu;

namespace TurnBase
{
    public class OrGameOverCondition: IGameOverCondition
    {
        public OrGameOverCondition(params IGameOverCondition[] conditions)
        {
            this.Conditions = conditions?.ToList() ?? throw new ArgumentNullException(nameof(conditions));
        }

        public List<IGameOverCondition> Conditions { get; }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            return this.Conditions.Any(condition => condition.IsGameOver(mainField, rules));
        }
    }
}