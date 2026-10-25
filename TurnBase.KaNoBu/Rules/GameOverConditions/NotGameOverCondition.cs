using TurnBase.KaNoBu;

namespace TurnBase
{
    public class NotGameOverCondition: IGameOverCondition
    {
        public NotGameOverCondition(IGameOverCondition condition)
        {
            this.Condition = condition;
        }

        public IGameOverCondition Condition { get; }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            return !this.Condition.IsGameOver(mainField, rules);
        }
    }
}