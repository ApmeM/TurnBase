using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{
    public class ReplayGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> : 
        IGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField>
    {
        private readonly List<ICommunicationModel> events;
        private MultipleGameLogListener<TMoveNotificationModel, TField> gameLogListeners = new MultipleGameLogListener<TMoveNotificationModel, TField>();

        public string GameId => "replay_game";
        public Func<Task> playerTurnDelayAction;

        public ReplayGame(List<ICommunicationModel> events)
        {
            this.events = events;
        }

        public void AddGameLogListener(IGameEventListener<TMoveNotificationModel, TField> gameLogListener)
        {
            this.gameLogListeners.Add(gameLogListener);
        }

        public AddPlayerStatus AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> player)
        {
            // Replay cant not have real players.
            return AddPlayerStatus.MAX_PLAYERS_REACHED;
        }

        public async Task Play(CancellationToken token = default)
        {
            for (var i = 0; i < events.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                var gameEvent = events[i];
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
                else if (gameEvent is GameLogCurrentFieldCommunicationModel<TField> gameLogCurrentField)
                {
                    this.gameLogListeners.GameLogCurrentField(gameLogCurrentField.field);
                }
                else if (gameEvent is GamePlayerTurnCommunicationModel<TMoveNotificationModel> gamePlayerTurn)
                {
                    this.gameLogListeners.GamePlayerTurn(gamePlayerTurn.playerNumber, gamePlayerTurn.notification);
                    await playerTurnDelayAction?.Invoke();
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

        public void Disconnect(IGameEventListener<TMoveNotificationModel, TField> player)
        {
            this.events.Clear();
        }
    }
}