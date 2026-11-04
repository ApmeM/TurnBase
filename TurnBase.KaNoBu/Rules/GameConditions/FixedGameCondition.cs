using TurnBase.KaNoBu;

namespace TurnBase
{
    public class FixedGameCondition : IGameCondition
    {
        private readonly bool result;

        public FixedGameCondition(bool result)
        {
            this.result = result;
        }

        public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return this.result;
        }
    }
}