using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class PendingActionHub
{
    // ToDo: cleanup dictionaries for players when game is finished.
    private readonly Dictionary<string, Queue<object>> PendingModels = new Dictionary<string, Queue<object>>();
    private readonly Dictionary<string, TaskCompletionSource<object>> PendingResponses = new Dictionary<string, TaskCompletionSource<object>>();

    public void PushModel(string playerId, object model)
    {
        if (!PendingModels.TryGetValue(playerId, out var queue))
        {
            PendingModels[playerId] = new Queue<object>();
        }

        this.PendingModels[playerId].Enqueue(model);
    }

    public object PopModel(string playerId)
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
        this.PendingResponses[playerId] = new TaskCompletionSource<object>();
        return PendingResponses[playerId]
            .Task
            .ContinueWith(t => (T)t.Result);
    }

    public void ResolveResponse(string playerId, object response)
    {
        if (PendingResponses.TryGetValue(playerId, out var tcs))
        {
            tcs.SetResult(response);
            PendingResponses.Remove(playerId);
        }
    }
}
