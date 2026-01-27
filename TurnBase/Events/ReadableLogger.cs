using System.Collections.Generic;

namespace TurnBase
{
    public class ReadableLogger<TMoveNotificationModel> : IGameEventListener<TMoveNotificationModel>
    {
        private ILogger logger;

        public ReadableLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void GameStarted()
        {
            this.logger.Log("Game started.");
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
            this.logger.Log($"Player {playerNumber} disconnected.");
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
            this.logger.Log($"Player {playerNumber} initialized with name '{playerName}'.");
        }

        public void PlayersInitialized()
        {
            this.logger.Log("All players initialized.");
        }

        public void GameLogCurrentField(IField field)
        {
            this.logger.Log($"Current field state: \n{field}");
        }

        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
            this.logger.Log($"Player {playerNumber} moved: \n{notification}");
        }

        public void GameTurnFinished()
        {
            this.logger.Log("Turn finished.");
        }

        public void GameFinished(List<int> winners)
        {
            this.logger.Log($"Game finished. \nWinners: {string.Join(", ", winners)}");
        }
    }
}