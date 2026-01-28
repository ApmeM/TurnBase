using Godot;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

[SceneReference("Server.tscn")]
public partial class Server : IServer
{
    private class PendingActionHub
    {
        // ToDo: cleanup dictionaries for players when game is finished or player is disconnected.
        private readonly Dictionary<string, Queue<ICommunicationModel>> PendingModels = new Dictionary<string, Queue<ICommunicationModel>>();
        private readonly Dictionary<string, TaskCompletionSource<ICommunicationModel>> PendingResponses = new Dictionary<string, TaskCompletionSource<ICommunicationModel>>();

        public void PushModel(string playerId, ICommunicationModel model)
        {
            if (!PendingModels.ContainsKey(playerId))
            {
                PendingModels[playerId] = new Queue<ICommunicationModel>();
            }

            this.PendingModels[playerId].Enqueue(model);
        }

        public ICommunicationModel PopModel(string playerId)
        {
            if (!PendingModels.TryGetValue(playerId, out var queue))
            {
                return null;
            }
            if (queue.Count == 0)
            {
                return null;
            }
            var model = queue.Dequeue();
            return model;
        }

        public Task<T> WaitResponse<T>(string playerId)
        {
            this.PendingResponses[playerId] = new TaskCompletionSource<ICommunicationModel>();
            return PendingResponses[playerId]
                .Task
                .ContinueWith(t => (T)t.Result);
        }

        public void ResolveResponse(string playerId, ICommunicationModel response)
        {
            if (PendingResponses.TryGetValue(playerId, out var tcs))
            {
                tcs.SetResult(response);
                PendingResponses.Remove(playerId);
            }
        }
    }

    private readonly TCP_Server server = new TCP_Server();
    private const int Port = 8080;

    private readonly PendingActionHub Actions = new PendingActionHub();
    private readonly List<StreamPeerTCP> incomingPeers = new List<StreamPeerTCP>();
    private readonly List<(StreamPeerTCP, float, string)> waitingPeers = new List<(StreamPeerTCP, float, string)>();
    private readonly Stack<string> playerIds = new Stack<string>();

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public void StartServer()
    {
        if (server.IsListening())
        {
            return;
        }

        server.Listen(Port);
        GD.Print($"Server listening on {Port}");
    }

    public override void _Process(float delta)
    {
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

            var model = this.Actions.PopModel(playerId);
            if (model != null)
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

            if (request.StartsWith("GET /wait-action", System.StringComparison.InvariantCultureIgnoreCase))
            {
                waitingPeers.Add((peer, 30, GetQueryValue(request, "playerId")));
                continue;
            }
            else if (request.StartsWith("OPTIONS /answer", System.StringComparison.InvariantCultureIgnoreCase))
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
            else if (request.StartsWith("POST /answer", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var playerId = GetQueryValue(request, "playerId");
                var body = request.Split("\r\n\r\n")[1];
                var responseObj = CommunicationSerializer.DeserializeObject<ICommunicationModel>(body);
                this.Actions.ResolveResponse(playerId, responseObj);
                SendStatus(peer, 200, null); // OK
            }
            else if (request.StartsWith("Get /join", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var gameId = GetQueryValue(request, "gameId");

                if (playerIds.Count > 0)
                {
                    var playerId = playerIds.Pop();
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
        // ToDo: on server gameId should be used.
        this.playerIds.Push(playerId);
    }

    public void SendRequest(string playerId, ICommunicationModel model)
    {
        this.Actions.PushModel(playerId, model);
    }

    public Task<T> SendRequest<T>(string playerId, ICommunicationModel model)
    {
        this.Actions.PushModel(playerId, model);
        return this.Actions.WaitResponse<T>(playerId);
    }
}
