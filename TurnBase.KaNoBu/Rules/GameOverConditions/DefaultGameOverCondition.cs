using TurnBase.KaNoBu;

namespace TurnBase
{
    public class DefaultGameOverCondition : IGameOverCondition
    {
        public bool IsGameOver(KaNoBuField kaNoBuField, KaNoBuRules rules)
        {
            var mainField = kaNoBuField.Field;
            var mainWidth = mainField.Width;
            var mainHeight = mainField.Height;

            int? playerLeft = null;
            for (var i = 0; i < mainWidth; i++)
            {
                for (var j = 0; j < mainHeight; j++)
                {
                    var playerShip = (KaNoBuFigure)mainField.GetFigure(i, j);
                    if (playerShip != null &&
                        !kaNoBuField.DisconnectedPlayers.Contains(playerShip.PlayerId) &&
                        playerShip.IsMoveable)
                    {
                        if (playerLeft == null)
                        {
                            playerLeft = playerShip.PlayerId;
                        }
                        else if (playerLeft != playerShip.PlayerId)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}