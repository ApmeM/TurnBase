using Godot;
using System.Text;
using System.Threading.Tasks;
using TurnBase;


public class RemoteGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> :
    IGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
{
    private readonly Client client;
    private readonly string serverUrl;
    private readonly string gameId;
    private IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player;
    private MultipleGameLogListener<TMoveNotificationModel> gameLogListeners = new MultipleGameLogListener<TMoveNotificationModel>();

    public RemoteGame(Client client, string serverUrl, string gameId)
    {
        this.client = client;
        this.serverUrl = serverUrl;
        this.gameId = gameId;
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
        var gameIdQueryString = new Godot.Collections.Dictionary { { "gameId", gameId } };
        var playerId = ((await SendAction("join", gameIdQueryString)).body as JoinGameResponseModel).PlayerId;
        GD.Print($"Joined as {playerId}");

        var playerIdQueryString = new Godot.Collections.Dictionary { { "playerId", playerId } };

        while (true)
        {
            GD.Print($"Sending wait action.");
            var result = await SendAction("wait-action", playerIdQueryString);

            if (result.code == 0)
            {
                break;
            }

            if (result.code == 200)
            {
                GD.Print($"It is {result.body?.GetType()?.Name ?? "UNKNOWN"}");
                if (result.body is InitModel<TInitModel> init)
                {
                    var response = await player.Init(init);
                    await this.SendAction("answer", playerIdQueryString, response);
                }
                else if (result.body is MakeTurnModel<TMoveModel> turn)
                {
                    var response = await player.MakeTurn(turn);
                    await this.SendAction("answer", playerIdQueryString, response);
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
                    GD.PushError("Unknown model from server");
                }
            }
        }
    }

    private struct Response
    {
        public int result;
        public int code;
        public string[] headers;
        public ICommunicationModel body;
    }


    private async Task<Response> SendAction(string action, Godot.Collections.Dictionary playerId, ICommunicationModel body = null)
    {
        var queryString = new HTTPClient().QueryStringFromDict(playerId);
        var url = $"{this.serverUrl}/{action}?{queryString}";
        var stringBody = (body != null) ? CommunicationSerializer.SerializeObject(body) : null;
        var result = await this.client.SendRequest(url, stringBody);
        var response = Encoding.UTF8.GetString((byte[])result[3]);
        GD.Print($"Received response with code {(int)result[1]}: {response}");

        if ((int)result[1] != 200)
        {
            return new Response
            {
                result = (int)result[0],
                code = (int)result[1],
                headers = (string[])result[2],
                body = null
            };
        }

        return new Response
        {
            result = (int)result[0],
            code = (int)result[1],
            headers = (string[])result[2],
            body = CommunicationSerializer.DeserializeObject<ICommunicationModel>(response)
        };
    }
}
