using System.Collections.Generic;

namespace TurnBase
{
    
    public class MemoryStorageEventListener<TMoveNotificationModel> : IGameEventListener<TMoveNotificationModel>
    {
        public readonly List<ICommunicationModel> Events = new List<ICommunicationModel>();

        public void GameStarted()
        {
            this.Events.Add(new GameStartedCommunicationModel());
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
            this.Events.Add(new GamePlayerDisconnectedCommunicationModel { playerNumber = playerNumber });
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
            this.Events.Add(new GamePlayerInitCommunicationModel { playerName = playerName, playerNumber = playerNumber });
        }

        public void PlayersInitialized()
        {
            this.Events.Add(new GamePlayersInitializedCommunicationModel());
        }

        public void GameLogCurrentField(IField field)
        {
            this.Events.Add(new GameLogCurrentFieldCommunicationModel { field = field });
        }

        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
            this.Events.Add(new GamePlayerTurnCommunicationModel<TMoveNotificationModel > { playerNumber = playerNumber, notification = notification });
        }

        public void GameTurnFinished()
        {
            this.Events.Add(new GameTurnFinishedCommunicationModel());
        }

        public void GameFinished(List<int> winners)
        {
            this.Events.Add(new GameFinishedCommunicationModel { winners = winners });
        }
    }
}