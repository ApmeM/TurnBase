using TurnBase.KaNoBu;

namespace TurnBase
{
    public class AllPlayersEliminatedGameOverCondition: IGameOverCondition
    {
        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            return GameOverConditionHelpers.CountActivePlayers(mainField, rules) == 0;
        }
    }
}