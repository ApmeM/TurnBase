using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class PendingActionHub
{
    private readonly Dictionary<string, object> PendingModels = new Dictionary<string, object>();
    private readonly Dictionary<string, TaskCompletionSource<object>> PendingResponses = new Dictionary<string, TaskCompletionSource<object>>();

    public void PushModel(string playerId, object model)
    {
        this.PendingModels[playerId] = model;
    }

    public object PopModel(string playerId)
    {
        if (PendingModels.TryGetValue(playerId, out var model))
        {
            PendingModels.Remove(playerId);
            return model;
        }
        return null;
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
