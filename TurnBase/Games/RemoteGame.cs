using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurnBase
{
    public struct ClientResponse
    {
        public int result;
        public int code;
        public string[] headers;
        public ICommunicationModel body;
    }

    public interface IClient
    {
        Task<ClientResponse> SendAction(string serverUrl, string action, Dictionary<string, object> queryData, ICommunicationModel body = null);
    }

    public class RemoteGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> :
        IGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
    {
        private readonly IClient client;
        private readonly string serverUrl;
        public string GameId { get; private set; }
        private IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player;
        private MultipleGameLogListener<TMoveNotificationModel> gameLogListeners = new MultipleGameLogListener<TMoveNotificationModel>();
        private bool connected;

        public RemoteGame(IClient client, string serverUrl, string gameId)
        {
            this.client = client;
            this.serverUrl = serverUrl;
            this.GameId = gameId;
        }

        public AddPlayerStatus AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player)
        {
            // Remote game supports only one player.
            if (this.player == null)
            {
                this.player = player;
                return AddPlayerStatus.OK;
            }

            return AddPlayerStatus.MAX_PLAYERS_REACHED;
        }

        public void AddGameLogListener(IGameEventListener<TMoveNotificationModel> gameLogListener)
        {
            this.gameLogListeners.Add(gameLogListener);
        }

        public async Task Play()
        {
            var gameIdQueryString = new Dictionary<string, object> { { "gameId", this.GameId } };
            var playerId = ((await this.client.SendAction(serverUrl, "join", gameIdQueryString)).body as JoinGameResponseModel).PlayerId;

            var playerIdQueryString = new Dictionary<string, object> { { "playerId", playerId } };

            this.connected = true;

            while (this.connected)
            {
                var result = await this.client.SendAction(serverUrl, "wait-action", playerIdQueryString);

                if (result.code == 0)
                {
                    break;
                }

                if (result.code == 200)
                {
                    if (result.body is InitModel<TInitModel> init)
                    {
                        var response = await player.Init(init);
                        await this.client.SendAction(serverUrl, "answer", playerIdQueryString, response);
                    }
                    else if (result.body is MakeTurnModel<TMoveModel> turn)
                    {
                        var response = await player.MakeTurn(turn);
                        await this.client.SendAction(serverUrl, "answer", playerIdQueryString, response);
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
                    else if (result.body is GameLogCurrentFieldCommunicationModel gameLogCurrentField)
                    {
                        this.player.GameLogCurrentField(gameLogCurrentField.field);
                        this.gameLogListeners.GameLogCurrentField(gameLogCurrentField.field);
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

        public void Disconnect(IGameEventListener<TMoveNotificationModel> player)
        {
            this.connected = false;
        }
    }
}