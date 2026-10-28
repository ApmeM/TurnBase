using TurnBase.KaNoBu;

namespace TurnBase
{
    public interface ILooseCondition
    {
        bool IsLooser(KaNoBuField mainField, KaNoBuRules rules, int playerNumber);
    }
}