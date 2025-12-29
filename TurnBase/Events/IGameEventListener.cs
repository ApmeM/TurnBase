using System.Collections.Generic;

namespace TurnBase
{
    public interface IGameEventListener<TMoveNotificationModel>
    {
        void GameStarted();
        void GamePlayerInitialized(int playerNumber, string playerName);
        void GamePlayerWrongTurn(int playerNumber, MoveValidationStatus status);
        void GameTurnFinished();
        void GamePlayerDisconnected(int playerNumber);
        void GameFinished(List<int> winners);
        void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification);
    }
}