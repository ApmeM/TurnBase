using Godot;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using TurnBase;
using System.Threading;
using System;

[SceneReference("GodotClient.tscn")]
public partial class GodotClient : IClient
{
    private HTTPClient httpClient = new HTTPClient();

    public async Task<ClientResponse> SendAction(string serverUrl, string action, Dictionary<string, object> queryData, ICommunicationModel body, CancellationToken token)
    {
        var queryString = httpClient.QueryStringFromDict(ToGodotDictionaryRecursive(queryData));
        var url = $"{serverUrl}/{action}?{queryString}";
        var stringBody = (body != null) ? CommunicationSerializer.SerializeObject(body) : null;
        var result = await this.SendRequest(url, stringBody, token);
        var response = Encoding.UTF8.GetString(result.Item4);
        GD.Print($"Received response with code {result.Item2}: {response}");

        if (result.Item2 != 200)
        {
            return new ClientResponse
            {
                result = result.Item1,
                code = result.Item2,
                headers = result.Item3,
                body = null
            };
        }

        return new ClientResponse
        {
            result = result.Item1,
            code = result.Item2,
            headers = result.Item3,
            body = CommunicationSerializer.DeserializeObject<ICommunicationModel>(response)
        };
    }

    private async Task<(int,int,string[], byte[])> SendRequest(string url, string body, CancellationToken token)
    {
        if (body != null)
        {
            this.http.Request(
                url,
                new[] { "Content-Type: application/json" },
                false,
                HTTPClient.Method.Post,
                body);
        }
        else
        {
            this.http.Request(url);
        }

        try
        {
            var result = await this.http
                .ToMySignal<int,int,string[], byte[]>("request_completed")
                .WrapCancellation(token);

            return result;
        }
        catch (OperationCanceledException)
        {
            this.http.CancelRequest();
            throw;
        }
    }

    private static Godot.Collections.Dictionary ToGodotDictionaryRecursive(IDictionary source)
    {
        var gdDict = new Godot.Collections.Dictionary();

        foreach (DictionaryEntry entry in source)
        {
            gdDict[entry.Key] = ConvertValue(entry.Value);
        }

        return gdDict;
    }

    private static object ConvertValue(object value)
    {
        if (value is IDictionary dict)
            return ToGodotDictionaryRecursive(dict);

        if (value is IList list)
        {
            var gdArray = new Godot.Collections.Array();
            foreach (var item in list)
                gdArray.Add(ConvertValue(item));
            return gdArray;
        }

        return value; // primitives, strings, etc.
    }

}
