using TurnBase.KaNoBu;

namespace TurnBase
{
    internal static class GameOverConditionHelpers
    {
        public static int CountActivePlayers(KaNoBuField mainField, KaNoBuRules rules)
        {
            var activePlayersCount = 0;
            for (var playerNumber = 0; playerNumber < rules.GetMaxPlayersCount(); playerNumber++)
            {
                if (rules.AutoMove(mainField, playerNumber) == null)
                {
                    activePlayersCount++;
                }
            }

            return activePlayersCount;
        }
    }
}