using TurnBase.KaNoBu;

namespace TurnBase
{
    public class DefaultVictoryCondition : IVictoryCondition
    {
        public bool IsWinner(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return rules.AutoMove(mainField, playerNumber) == null;
        }
    }
}