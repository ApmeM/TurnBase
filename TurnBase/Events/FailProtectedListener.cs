using System.Collections.Generic;

namespace TurnBase
{
    public class FailProtectedListener<TMoveNotificationModel> : 
        IGameEventListener<TMoveNotificationModel>
    {
        private readonly IGameEventListener<TMoveNotificationModel> listener;

        public FailProtectedListener(IGameEventListener<TMoveNotificationModel> listener)
        {
            this.listener = listener;
        }

        public void GameStarted()
        {
            try
            {
                this.listener.GameStarted();
            }
            catch
            {

            }
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
            try
            {
                this.listener.GamePlayerDisconnected(playerNumber);
            }
            catch
            {

            }
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
            try
            {
                this.listener.GamePlayerInit(playerNumber, playerName);
            }
            catch
            {

            }
        }

        public void GamePlayersInitialized()
        {
            try
            {
                this.listener.GamePlayersInitialized();
            }
            catch
            {

            }
        }

        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
            try
            {
                this.listener.GamePlayerTurn(playerNumber, notification);
            }
            catch
            {

            }
        }

        public void GameTurnFinished()
        {
            try
            {
                this.listener.GameTurnFinished();
            }
            catch
            {

            }
        }

        public void GameFinished(List<int> winners)
        {
            try
            {
                this.listener.GameFinished(winners);
            }
            catch
            {

            }
        }
    }
}