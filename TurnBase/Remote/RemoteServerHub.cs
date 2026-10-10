using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{

    /// <summary>
    /// Shared, transport-independent state and request coordination for remote players.
    /// Network hosts should only translate their transport's requests into these operations.
    /// </summary>
    public sealed class RemoteServerHub
    {
        private sealed class PlayerChannel
        {
            public readonly ConcurrentQueue<ICommunicationModel> Requests = new ConcurrentQueue<ICommunicationModel>();
            public readonly SemaphoreSlim RequestSignal = new SemaphoreSlim(0);
            public readonly object ResponseLock = new object();
            public TaskCompletionSource<ICommunicationModel> PendingResponse;
        }

        private readonly ConcurrentDictionary<string, PlayerChannel> players = new ConcurrentDictionary<string, PlayerChannel>();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> availablePlayers = new ConcurrentDictionary<string, ConcurrentQueue<string>>();

        public void RegisterPlayer(string playerId, string gameId)
        {
            var queue = this.availablePlayers.GetOrAdd(gameId, _ => new ConcurrentQueue<string>());
            queue.Enqueue(playerId);
            this.players.GetOrAdd(playerId, _ => new PlayerChannel());
        }

        public bool TryJoin(string gameId, out string playerId)
        {
            playerId = null;
            if (!string.IsNullOrWhiteSpace(gameId))
            {
                return this.availablePlayers.TryGetValue(gameId, out var queue) && queue.TryDequeue(out playerId);
            }

            foreach (var queue in this.availablePlayers.Values)
            {
                if (queue.TryDequeue(out playerId))
                {
                    return true;
                }
            }

            return false;
        }

        public void SendRequest(string playerId, ICommunicationModel model)
        {
            var channel = this.GetPlayerChannel(playerId);
            channel.Requests.Enqueue(model);
            channel.RequestSignal.Release();
        }

        public async Task<T> SendRequest<T>(string playerId, ICommunicationModel model, CancellationToken token = default)
        {
            var channel = this.GetPlayerChannel(playerId);
            var response = new TaskCompletionSource<ICommunicationModel>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (channel.ResponseLock)
            {
                if (channel.PendingResponse != null)
                {
                    throw new InvalidOperationException("A response is already pending for this remote player.");
                }

                channel.PendingResponse = response;
            }

            using (token.Register(() => response.TrySetCanceled()))
            {
                this.SendRequest(playerId, model);
                try
                {
                    return (T)(object)await response.Task.ConfigureAwait(false);
                }
                finally
                {
                    lock (channel.ResponseLock)
                    {
                        if (ReferenceEquals(channel.PendingResponse, response))
                        {
                            channel.PendingResponse = null;
                        }
                    }
                }
            }
        }

        public bool TryTakeRequest(string playerId, out ICommunicationModel model)
        {
            model = null;
            if (!this.players.TryGetValue(playerId, out var channel) || !channel.RequestSignal.Wait(0))
            {
                return false;
            }

            return channel.Requests.TryDequeue(out model);
        }

        public async Task<ICommunicationModel> WaitForRequestAsync(string playerId, TimeSpan timeout, CancellationToken token = default)
        {
            var channel = this.GetPlayerChannel(playerId);
            if (!await channel.RequestSignal.WaitAsync(timeout, token).ConfigureAwait(false))
            {
                return null;
            }

            return channel.Requests.TryDequeue(out var model) ? model : null;
        }

        public bool SubmitResponse(string playerId, ICommunicationModel response)
        {
            if (!this.players.TryGetValue(playerId, out var channel))
            {
                return false;
            }

            TaskCompletionSource<ICommunicationModel> pendingResponse;
            lock (channel.ResponseLock)
            {
                pendingResponse = channel.PendingResponse;
            }

            return pendingResponse != null && pendingResponse.TrySetResult(response);
        }

        private PlayerChannel GetPlayerChannel(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new ArgumentException("Player ID cannot be empty.", nameof(playerId));
            }

            return this.players.GetOrAdd(playerId, _ => new PlayerChannel());
        }
    }
}
