using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{

    public class RemoteGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> :
        IGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField>
    {
        private readonly IClient client;
        private readonly string serverUrl;
        public string GameId { get; private set; }
        private IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> player;
        private MultipleGameLogListener<TMoveNotificationModel, TField> gameLogListeners = new MultipleGameLogListener<TMoveNotificationModel, TField>();
        private bool connected;

        public RemoteGame(IClient client, string serverUrl, string gameId)
        {
            this.client = client;
            this.serverUrl = serverUrl;
            this.GameId = gameId;
        }

        public AddPlayerStatus AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> player)
        {
            // Remote game supports only one player.
            if (this.player == null)
            {
                this.player = player;
                return AddPlayerStatus.OK;
            }

            return AddPlayerStatus.MAX_PLAYERS_REACHED;
        }

        public void AddGameLogListener(IGameEventListener<TMoveNotificationModel, TField> gameLogListener)
        {
            this.gameLogListeners.Add(gameLogListener);
        }

        public async Task Play(CancellationToken token = default)
        {
            var gameIdQueryString = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(this.GameId))
            {
                gameIdQueryString.Add("gameId", this.GameId);
            }

            var joinResult = await this.client.SendAction(serverUrl, RemoteProtocol.JoinAction, gameIdQueryString, null, token);
            if (joinResult.code != 200 || !(joinResult.body is JoinGameResponseModel joinResponse) || string.IsNullOrWhiteSpace(joinResponse.PlayerId))
            {
                throw new System.InvalidOperationException($"Could not join game '{this.GameId}' (HTTP {joinResult.code}).");
            }

            var playerId = joinResponse.PlayerId;

            var playerIdQueryString = new Dictionary<string, object> { { "playerId", playerId } };

            this.connected = true;

            while (this.connected)
            {
                token.ThrowIfCancellationRequested();
                var result = await this.client.SendAction(serverUrl, RemoteProtocol.WaitAction, playerIdQueryString, null, token);

                if (result.code == 0)
                {
                    break;
                }

                if (result.code == 200)
                {
                    if (result.body is InitModel<TInitModel> init)
                    {
                        var response = await player.Init(init, token);
                        await this.client.SendAction(serverUrl, RemoteProtocol.AnswerAction, playerIdQueryString, response, token);
                    }
                    else if (result.body is MakeTurnModel<TMoveModel> turn)
                    {
                        var response = await player.MakeTurn(turn, token);
                        await this.client.SendAction(serverUrl, RemoteProtocol.AnswerAction, playerIdQueryString, response, token);
                    }
                    else if (result.body is GameStartedCommunicationModel gameStarted)
                    {
                        this.player.GameStarted();
                        this.gameLogListeners.GameStarted();
                    }
                    else if (result.body is GamePlayerInitCommunicationModel gamePlayerInit)
                    {
                        this.player.GamePlayerInit(gamePlayerInit.playerNumber, gamePlayerInit.playerName);
                        this.gameLogListeners.GamePlayerInit(gamePlayerInit.playerNumber, gamePlayerInit.playerName);
                    }
                    else if (result.body is GamePlayersInitializedCommunicationModel gamePlayersInitialized)
                    {
                        this.player.PlayersInitialized();
                        this.gameLogListeners.PlayersInitialized();
                    }
                    else if (result.body is GamePlayerTurnCommunicationModel<TMoveNotificationModel> gamePlayerTurn)
                    {
                        this.player.GamePlayerTurn(gamePlayerTurn.playerNumber, gamePlayerTurn.notification);
                        this.gameLogListeners.GamePlayerTurn(gamePlayerTurn.playerNumber, gamePlayerTurn.notification);
                    }
                    else if (result.body is GameTurnFinishedCommunicationModel gameTurnFinished)
                    {
                        this.player.GameTurnFinished();
                        this.gameLogListeners.GameTurnFinished();
                    }
                    else if (result.body is GamePlayerDisconnectedCommunicationModel gamePlayerDisconnected)
                    {
                        this.player.GamePlayerDisconnected(gamePlayerDisconnected.playerNumber);
                        this.gameLogListeners.GamePlayerDisconnected(gamePlayerDisconnected.playerNumber);
                    }
                    else if (result.body is GameFinishedCommunicationModel gameFinished)
                    {
                        this.player.GameFinished(gameFinished.winners);
                        this.gameLogListeners.GameFinished(gameFinished.winners);
                        break;
                    }
                    else
                    {
                        throw new System.Exception($"Unknown communication model type: {result.body?.GetType()?.Name ?? "null"}");
                    }
                }
            }
        }

        public void Disconnect(IGameEventListener<TMoveNotificationModel, TField> player)
        {
            this.connected = false;
        }
    }
}