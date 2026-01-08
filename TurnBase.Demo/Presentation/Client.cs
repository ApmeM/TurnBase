using Godot;
using System.Text;
using System.Threading.Tasks;
using TurnBase;

public class Client : Node
{
    private struct Response
    {
        public int result;
        public int code;
        public string[] headers;
        public object body;
    }

    [Export]
    public string ServerUrl = "http://localhost:8080";

    public async Task StartPolling<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>(
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> player, string gameId)
    {
        var gameIdQueryString = new Godot.Collections.Dictionary { { "gameId", gameId } };
        var playerId = (await SendAction("join", gameIdQueryString)).body;
        GD.Print($"Joined as {playerId}");

        var playerIdQueryString = new Godot.Collections.Dictionary { { "playerId", playerId } };

        while (true)
        {
            GD.Print($"Sending wait action.");
            var result = await SendAction("wait-action", playerIdQueryString);
            GD.Print($"Received wait action response with code {result.code}");

            if (result.code == 200)
            {
                if (result.body is InitModel<TInitModel> init)
                {
                    GD.Print($"It is init");
                    var response = await player.Init(init);
                    await this.SendAction("answer", playerIdQueryString, response);
                }
                else if (result.body is MakeTurnModel<TMoveModel> turn)
                {
                    GD.Print($"It is move");
                    var response = await player.MakeTurn(turn);
                    await this.SendAction("answer", playerIdQueryString, response);
                }
                else
                {
                    GD.PushError("Unknown model from server");
                }
            }
        }
    }

    private async Task<Response> SendAction(string action, Godot.Collections.Dictionary playerId, object body = null)
    {
        var req = this.GetNode<HTTPRequest>("Http");

        var queryString = new HTTPClient().QueryStringFromDict(playerId);
        var url = $"{ServerUrl}/{action}?{queryString}";
        if (body != null)
        {
            req.Request(
                url,
                new[] { "Content-Type: application/json" },
                false,
                HTTPClient.Method.Post,
                CommunicationSerializer.SerializeObject(body)
            );
        }
        else
        {
            req.Request(url);
        }

        var result = await ToSignal(req, "request_completed");

        return new Response
        {
            result = (int)result[0],
            code = (int)result[1],
            headers = (string[])result[2],
            body = CommunicationSerializer.DeserializeObject<object>(Encoding.UTF8.GetString((byte[])result[3]))
        };
    }
}
