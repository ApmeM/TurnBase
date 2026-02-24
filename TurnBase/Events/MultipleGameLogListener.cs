using System.Collections.Generic;

namespace TurnBase
{
    public class MultipleGameLogListener<TMoveNotificationModel> : IGameEventListener<TMoveNotificationModel>
    {
        private List<IGameEventListener<TMoveNotificationModel>> gameLogListeners = new List<IGameEventListener<TMoveNotificationModel>>();

        public void Add(IGameEventListener<TMoveNotificationModel> gameLogListener)
        {
            this.gameLogListeners.Add(gameLogListener);
        }

        public void Remove(IGameEventListener<TMoveNotificationModel> gameLogListener)
        {
            this.gameLogListeners.Remove(gameLogListener);
        }

        public void GameStarted()
        {
            this.gameLogListeners.ForEach(a => a.GameStarted());
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
            this.gameLogListeners.ForEach(a => a.GamePlayerDisconnected(playerNumber));
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
            this.gameLogListeners.ForEach(a => a.GamePlayerInit(playerNumber, playerName));
        }

        public void PlayersInitialized()
        {
            this.gameLogListeners.ForEach(a => a.PlayersInitialized());
        }

        public void GameLogCurrentField(IField field)
        {
            this.gameLogListeners.ForEach(a => a.GameLogCurrentField(field));
        }

        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
            this.gameLogListeners.ForEach(a => a.GamePlayerTurn(playerNumber, notification));
        }

        public void GameTurnFinished()
        {
            this.gameLogListeners.ForEach(a => a.GameTurnFinished());
        }

        public void GameFinished(List<int> winners)
        {
            this.gameLogListeners.ForEach(a => a.GameFinished(winners));
        }
    }
}