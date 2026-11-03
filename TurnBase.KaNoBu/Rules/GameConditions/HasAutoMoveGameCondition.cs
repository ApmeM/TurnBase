using TurnBase.KaNoBu;

namespace TurnBase
{
    public class HasAutoMoveGameCondition : IGameCondition
    {
        public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return rules.AutoMove(mainField, playerNumber) != null;
        }
    }
}