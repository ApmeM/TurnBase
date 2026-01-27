using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurnBase
{
    public class ReplayGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : 
        IGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
    {
        private readonly List<ICommunicationModel> events;
        private MultipleGameLogListener<TMoveNotificationModel> gameLogListeners = new MultipleGameLogListener<TMoveNotificationModel>();

        public ReplayGame(List<ICommunicationModel> events)
        {
            this.events = events;
        }

        public void AddGameLogListener(IGameEventListener<TMoveNotificationModel> gameLogListener)
        {
            this.gameLogListeners.Add(gameLogListener);
        }

        public AddPlayerStatus AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player)
        {
            // Replay cant not have real players.
            return AddPlayerStatus.MAX_PLAYERS_REACHED;
        }

        public async Task Play()
        {
            foreach (var gameEvent in events)
            {
                if (gameEvent is GameStartedCommunicationModel gameStarted)
                {
                    this.gameLogListeners.GameStarted();
                }
                else if (gameEvent is GamePlayerInitCommunicationModel gamePlayerInit)
                {
                    this.gameLogListeners.GamePlayerInit(gamePlayerInit.playerNumber, gamePlayerInit.playerName);
                }
                else if (gameEvent is GamePlayersInitializedCommunicationModel gamePlayersInitialized)
                {
                    this.gameLogListeners.PlayersInitialized();
                }
                else if (gameEvent is GameLogCurrentFieldCommunicationModel gameLogCurrentField)
                {
                    this.gameLogListeners.GameLogCurrentField(gameLogCurrentField.field);
                }
                else if (gameEvent is GamePlayerTurnCommunicationModel<TMoveNotificationModel> gamePlayerTurn)
                {
                    this.gameLogListeners.GamePlayerTurn(gamePlayerTurn.playerNumber, gamePlayerTurn.notification);
                }
                else if (gameEvent is GameTurnFinishedCommunicationModel gameTurnFinished)
                {
                    this.gameLogListeners.GameTurnFinished();
                }
                else if (gameEvent is GamePlayerDisconnectedCommunicationModel gamePlayerDisconnected)
                {
                    this.gameLogListeners.GamePlayerDisconnected(gamePlayerDisconnected.playerNumber);
                }
                else if (gameEvent is GameFinishedCommunicationModel gameFinished)
                {
                    this.gameLogListeners.GameFinished(gameFinished.winners);
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