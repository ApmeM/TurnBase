using Godot;
using System.Text;
using System.Threading.Tasks;
using TurnBase;

public class RemoteGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : IGame
{
    private readonly Client client;
    private readonly string serverUrl;
    private readonly string gameId;
    private IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player;

    public RemoteGame(Client client, string serverUrl, string gameId)
    {
        this.client = client;
        this.serverUrl = serverUrl;
        this.gameId = gameId;
    }

    public void SetPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player)
    {
        this.player = player;
    }

    public async Task Play()
    {
        var gameIdQueryString = new Godot.Collections.Dictionary { { "gameId", gameId } };
        var playerId = (await SendAction("join", gameIdQueryString)).body;
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
                    player.GameStarted();
                }
                else if (result.body is GamePlayerInitCommunicationModel gamePlayerInit)
                {
                    player.GamePlayerInit(gamePlayerInit.playerNumber, gamePlayerInit.playerName);
                }
                else if (result.body is GamePlayersInitializedCommunicationModel gamePlayersInitialized)
                {
                    player.PlayersInitialized(gamePlayersInitialized.mainField);
                }
                else if (result.body is GamePlayerTurnCommunicationModel gamePlayerTurn)
                {
                    player.GamePlayerTurn(gamePlayerTurn.playerNumber, (TMoveNotificationModel)gamePlayerTurn.notification);
                }
                else if (result.body is GameTurnFinishedCommunicationModel gameTurnFinished)
                {
                    player.GameTurnFinished();
                }
                else if (result.body is GamePlayerDisconnectedCommunicationModel gamePlayerDisconnected)
                {
                    player.GamePlayerDisconnected(gamePlayerDisconnected.playerNumber);
                }
                else if (result.body is GameFinishedCommunicationModel gameFinished)
                {
                    player.GameFinished(gameFinished.winners);
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
        public object body;
    }


    private async Task<Response> SendAction(string action, Godot.Collections.Dictionary playerId, object body = null)
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
            body = CommunicationSerializer.DeserializeObject<object>(response)
        };
    }
}
