using System.Net;
using System.Text;

namespace TurnBase.KaNoBu;

public class ConsoleNetworkServer : IDisposable, IServer
{
    private readonly HttpListener listener = new();
    private readonly CancellationTokenSource shutdown = new();
    private readonly RemoteServerHub hub = new RemoteServerHub();

    private void StartServer(int port)
    {
        this.listener.Prefixes.Add($"http://+:{port}/");
        if (this.listener.IsListening)
        {
            return;
        }

        this.listener.Start();
        this.AcceptRequestsAsync();
    }

    public void Dispose()
    {
        this.shutdown.Cancel();
        this.listener.Close();
        this.shutdown.Dispose();
    }

    private async void AcceptRequestsAsync()
    {
        while (!this.shutdown.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await this.listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException) when (this.shutdown.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            this.HandleRequestAsync(context);
        }
    }

    private async void HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var playerId = request.QueryString["playerId"];
            var response = context.Response;
            response.Headers["Access-Control-Allow-Origin"] = "*";
            response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
            response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";

            if (request.HttpMethod == "OPTIONS" && request.Url?.AbsolutePath == "/" + RemoteProtocol.AnswerAction)
            {
                response.StatusCode = (int)HttpStatusCode.NoContent;
                return;
            }
            
            if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/" + RemoteProtocol.JoinAction)
            {
                if (!this.hub.TryJoin(request.QueryString["gameId"] ?? string.Empty, out var nextPlayerId))
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                await WriteModelAsync(response, HttpStatusCode.OK, new JoinGameResponseModel { PlayerId = nextPlayerId }).ConfigureAwait(false);
                return;
            }
            
            if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/" + RemoteProtocol.WaitAction)
            {
                if (string.IsNullOrWhiteSpace(playerId))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                var model = await this.hub.WaitForRequestAsync(playerId, TimeSpan.FromSeconds(30), this.shutdown.Token).ConfigureAwait(false);
                if (model == null)
                {
                    response.StatusCode = (int)HttpStatusCode.NoContent;
                    return;
                }
                await WriteModelAsync(response, HttpStatusCode.OK, model).ConfigureAwait(false);
                return;
            }

            if (request.HttpMethod == "POST" && request.Url?.AbsolutePath == "/" + RemoteProtocol.AnswerAction)
            {
                if (string.IsNullOrWhiteSpace(playerId))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);
                var answer = CommunicationSerializer.DeserializeObject<ICommunicationModel>(body);
                if (answer == null || !this.hub.SubmitResponse(playerId, answer))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                return;
            }

            response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        catch (HttpListenerException)
        {
            // The client disconnected while the request was being processed.
        }
        catch (ObjectDisposedException)
        {
            // The server is shutting down.
        }
        catch (OperationCanceledException) when (this.shutdown.IsCancellationRequested)
        {
            // A long-poll request was cancelled during server shutdown.
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Ошибка HTTP-сервера: {exception.Message}");
            try
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            catch (ObjectDisposedException)
            {
            }
        }
        finally
        {
            try
            {
                context.Response.Close();
            }
            catch (HttpListenerException)
            {
            }
        }
    }

    private static async Task WriteModelAsync(HttpListenerResponse response, HttpStatusCode status, ICommunicationModel model)
    {
        var body = CommunicationSerializer.SerializeObject(model);
        var bytes = Encoding.UTF8.GetBytes(body);
        response.StatusCode = (int)status;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
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
