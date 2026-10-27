using TurnBase.KaNoBu;

namespace TurnBase
{
    public interface IVictoryCondition
    {
        bool IsWinner(KaNoBuField mainField, KaNoBuRules rules, int playerNumber);
    }
}