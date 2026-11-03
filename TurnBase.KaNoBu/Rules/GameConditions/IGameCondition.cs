using TurnBase.KaNoBu;

namespace TurnBase
{
    public interface IGameCondition
    {
        bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber);
    }
}