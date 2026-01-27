using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurnBase
{
    public class ReplayGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : IGame
    {
        private readonly List<ICommunicationModel> events;
        private List<IGameEventListener<TMoveNotificationModel>> gameLogListeners = new List<IGameEventListener<TMoveNotificationModel>>();

        public ReplayGame(List<ICommunicationModel> events)
        {
            this.events = events;
        }

        public void AddGameLogListener(IGameEventListener<TMoveNotificationModel> gameLogListener)
        {
            this.gameLogListeners.Add(gameLogListener);
        }

        public async Task Play()
        {
            foreach (var gameEvent in events)
            {
                if (gameEvent is GameStartedCommunicationModel gameStarted)
                {
                    this.gameLogListeners.ForEach(a => a.GameStarted());
                }
                else if (gameEvent is GamePlayerInitCommunicationModel gamePlayerInit)
                {
                    this.gameLogListeners.ForEach(a => a.GamePlayerInit(gamePlayerInit.playerNumber, gamePlayerInit.playerName));
                }
                else if (gameEvent is GamePlayersInitializedCommunicationModel gamePlayersInitialized)
                {
                    this.gameLogListeners.ForEach(a => a.PlayersInitialized());
                }
                else if (gameEvent is GameLogCurrentFieldCommunicationModel gameLogCurrentField)
                {
                    this.gameLogListeners.ForEach(a => a.GameLogCurrentField(gameLogCurrentField.field));
                }
                else if (gameEvent is GamePlayerTurnCommunicationModel<TMoveNotificationModel> gamePlayerTurn)
                {
                    this.gameLogListeners.ForEach(a => a.GamePlayerTurn(gamePlayerTurn.playerNumber, gamePlayerTurn.notification));
                }
                else if (gameEvent is GameTurnFinishedCommunicationModel gameTurnFinished)
                {
                    this.gameLogListeners.ForEach(a => a.GameTurnFinished());
                }
                else if (gameEvent is GamePlayerDisconnectedCommunicationModel gamePlayerDisconnected)
                {
                    this.gameLogListeners.ForEach(a => a.GamePlayerDisconnected(gamePlayerDisconnected.playerNumber));
                }
                else if (gameEvent is GameFinishedCommunicationModel gameFinished)
                {
                    this.gameLogListeners.ForEach(a => a.GameFinished(gameFinished.winners));
                    break;
                }
                else
                {
                    throw new System.Exception($"Incorrect event in event log: {gameEvent.GetType()}");
                }
            }
        }
    }
}