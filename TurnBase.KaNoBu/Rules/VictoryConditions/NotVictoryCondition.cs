using TurnBase.KaNoBu;

namespace TurnBase
{
    public class NotVictoryCondition : IVictoryCondition
    {
        public NotVictoryCondition(IVictoryCondition condition)
        {
            this.Condition = condition;
        }

        public IVictoryCondition Condition { get; }

        public bool IsWinner(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return !this.Condition.IsWinner(mainField, rules, playerNumber);
        }
    }
}