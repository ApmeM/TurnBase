using TurnBase.KaNoBu;

namespace TurnBase
{
    public interface IGameOverCondition
    {
        bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules);
    }
}