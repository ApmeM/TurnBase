using TurnBase.KaNoBu;

namespace TurnBase
{
    public class NotGameCondition : IGameCondition
    {
        public NotGameCondition(IGameCondition condition)
        {
            this.Condition = condition;
        }

        public IGameCondition Condition { get; }

        public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return !this.Condition.CheckCondition(mainField, rules, playerNumber);
        }
    }
}