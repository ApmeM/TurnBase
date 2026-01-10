using System.Collections.Generic;

namespace TurnBase
{
    public interface IGameEventListener<TMoveNotificationModel>
    {
        void GameStarted();
        void GamePlayerInit(int playerNumber, string playerName);
        void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification);
        void GameTurnFinished();
        void GamePlayerDisconnected(int playerNumber);
        void GameFinished(List<int> winners);
    }
}