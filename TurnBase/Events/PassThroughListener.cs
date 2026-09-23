using System.Collections.Generic;

namespace TurnBase
{
    public class PassThroughListener<TMoveNotificationModel, TField> : IGameEventListener<TMoveNotificationModel, TField>
    {
        private readonly IGameEventListener<TMoveNotificationModel, TField> listener;

        public PassThroughListener(IGameEventListener<TMoveNotificationModel, TField> listener)
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

        public void PlayersInitialized()
        {
            this.listener.PlayersInitialized();
        }

        public void GameLogCurrentField(TField field)
        {
            this.listener.GameLogCurrentField(field);
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