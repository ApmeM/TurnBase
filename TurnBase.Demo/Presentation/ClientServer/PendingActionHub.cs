using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class PendingActionHub
{
    // ToDo: cleanup dictionaries for players when game is finished or player is disconnected.
    private readonly Dictionary<string, Queue<ICommunicationModel>> PendingModels = new Dictionary<string, Queue<ICommunicationModel>>();
    private readonly Dictionary<string, TaskCompletionSource<ICommunicationModel>> PendingResponses = new Dictionary<string, TaskCompletionSource<ICommunicationModel>>();

    public void PushModel(string playerId, ICommunicationModel model)
    {
        if (!PendingModels.TryGetValue(playerId, out var queue))
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
