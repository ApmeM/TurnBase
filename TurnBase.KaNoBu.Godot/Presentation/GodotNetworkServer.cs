using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using TurnBase;

[SceneReference("GodotNetworkServer.tscn")]
public partial class GodotNetworkServer: IServer
{
    private readonly TCP_Server server = new TCP_Server();

    private readonly RemoteServerHub hub = new RemoteServerHub();
    private readonly List<StreamPeerTCP> incomingPeers = new List<StreamPeerTCP>();
    private readonly List<(StreamPeerTCP, float, string)> waitingPeers = new List<(StreamPeerTCP, float, string)>();

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    private void StartServer(int port)
    {
        if (server.IsListening())
        {
            return;
        }

        server.Listen((ushort)port);
        GD.Print($"Server listening on {port}");
    }

    public override void _Process(float delta)
    {
        if (!server.IsListening())
        {
            return;
        }

        ProcessConnection();
        ProcessIncomingRequests();
        ProcessWaitingPeers(delta);
    }

    private void ProcessWaitingPeers(float delta)
    {
        for (int i = waitingPeers.Count - 1; i >= 0; i--)
        {
            var (peer, timeout, playerId) = waitingPeers[i];

            if (peer.GetStatus() != StreamPeerTCP.Status.Connected)
            {
                waitingPeers.RemoveAt(i);
                continue;
            }

            if (this.hub.TryTakeRequest(playerId, out var model))
            {
                SendStatus(peer, 200, model); // OK
            }
            else
            {
                waitingPeers[i] = (peer, timeout - delta, playerId);
                if (timeout > 0)
                {
                    continue;
                }

                SendStatus(peer, 204, null); // No Content
            }

            peer.DisconnectFromHost();
            waitingPeers.RemoveAt(i);
        }
    }

    private void ProcessIncomingRequests()
    {
        // Process only peers that have data available to avoid reading incomplete requests
        for (int i = incomingPeers.Count - 1; i >= 0; i--)
        {
            var peer = incomingPeers[i];
            if (peer.GetStatus() != StreamPeerTCP.Status.Connected)
            {
                incomingPeers.RemoveAt(i);
                continue;
            }

            if (peer.GetAvailableBytes() == 0)
            {
                continue;
            }

            var request = peer.GetUtf8String(peer.GetAvailableBytes());
            GD.Print($"Request received: {request}");

            if (request.StartsWith($"GET /{RemoteProtocol.WaitAction}", System.StringComparison.InvariantCultureIgnoreCase))
            {
                waitingPeers.Add((peer, 30, GetQueryValue(request, "playerId")));
                continue;
            }
            else if (request.StartsWith($"OPTIONS /{RemoteProtocol.AnswerAction}", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var playerId = GetQueryValue(request, "playerId");
                // ToDo: validate playerId
                if (string.IsNullOrWhiteSpace(playerId))
                {
                    SendStatus(peer, 400, null); // Bad Request
                }
                else
                {
                    SendStatus(peer, 204, null); // OK
                }
            }
            else if (request.StartsWith($"POST /{RemoteProtocol.AnswerAction}", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var playerId = GetQueryValue(request, "playerId");
                var requestParts = request.Split(new[] { "\r\n\r\n" }, 2, System.StringSplitOptions.None);
                if (string.IsNullOrWhiteSpace(playerId) || requestParts.Length < 2)
                {
                    SendStatus(peer, 400, null);
                }
                else
                {
                    var responseObj = CommunicationSerializer.DeserializeObject<ICommunicationModel>(requestParts[1]);
                    SendStatus(peer, responseObj != null && this.hub.SubmitResponse(playerId, responseObj) ? 200 : 400, null);
                }
            }
            else if (request.StartsWith($"GET /{RemoteProtocol.JoinAction}", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var gameId = GetQueryValue(request, "gameId");

                if (this.hub.TryJoin(gameId, out var playerId))
                {
                    GD.Print($"Player {playerId} joined.");
                    SendStatus(peer, 200, new JoinGameResponseModel { PlayerId = playerId }); // OK
                }
                else
                {
                    SendStatus(peer, 404, null); // Not found
                }
            }
            else
            {
                SendStatus(peer, 404, null); // Not found
            }

            peer.DisconnectFromHost();
            incomingPeers.RemoveAt(i);
        }
    }

    private void ProcessConnection()
    {
        if (!server.IsConnectionAvailable())
        {
            return;
        }

        // Accept new connections but defer processing until data arrives
        var peer = server.TakeConnection();
        incomingPeers.Add(peer);
    }

    private void SendStatus(StreamPeerTCP peer, int status, ICommunicationModel model)
    {
        var body = CommunicationSerializer.SerializeObject(model);

        var header =
            $"HTTP/1.1 {status} OK\r\n" +
            "Content-Type: application/json\r\n" +
            "Access-Control-Allow-Headers: Content-Type\r\n" +
            "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
            "Access-Control-Allow-Origin: *\r\n" +
            $"Content-Length: {Encoding.UTF8.GetByteCount(body)}\r\n\r\n";

        GD.Print($"Response sent {body}");

        peer.PutData(Encoding.UTF8.GetBytes(header + body));
    }

    private string GetQueryValue(string req, string key)
    {
        var start = req.IndexOf(key + "=");
        if (start == -1) return null;

        start += key.Length + 1;
        var end = req.IndexOfAny(new[] { '&', ' ' }, start);
        return end == -1 ? req.Substring(start) : req.Substring(start, end - start);
    }

    public void RegisterPlayer(string playerId, string gameId)
    {
        this.StartServer(RemoteProtocol.DefaultPort);
        this.hub.RegisterPlayer(playerId, gameId);
    }

    public void SendRequest(string playerId, ICommunicationModel model)
    {
        this.hub.SendRequest(playerId, model);
    }

    public Task<T> SendRequest<T>(string playerId, ICommunicationModel model, CancellationToken token = default)
    {
        return this.hub.SendRequest<T>(playerId, model, token);
    }
}
