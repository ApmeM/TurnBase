using System.Collections.Generic;

namespace TurnBase
{
    public class PassThroughListener<TMoveNotificationModel> : IGameEventListener<TMoveNotificationModel>
    {
        private readonly IGameEventListener<TMoveNotificationModel> listener;

        public PassThroughListener(IGameEventListener<TMoveNotificationModel> listener)
        {
            this.listener = listener;
        }

        public void GameStarted()
        {
            this.listener.GameStarted();
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
            this.listener.GamePlayerDisconnected(playerNumber);
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
            this.listener.GamePlayerInit(playerNumber, playerName);
        }

        public void GamePlayersInitialized()
        {
            this.listener.GamePlayersInitialized();
        }

        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
            this.listener.GamePlayerTurn(playerNumber, notification);
        }

        public void GameTurnFinished()
        {
            this.listener.GameTurnFinished();
        }

        public void GameFinished(List<int> winners)
        {
            this.listener.GameFinished(winners);
        }
    }
}