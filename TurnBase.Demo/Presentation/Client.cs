using Godot;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using TurnBase;

[SceneReference("Client.tscn")]
public partial class Client : IClient
{
    private HTTPClient httpClient= new HTTPClient();

    public async Task<object[]> SendRequest(string url, string body)
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

        var result = await ToSignal(this.http, "request_completed");
        return result;
    }

    public async Task<ClientResponse> SendAction(string serverUrl, string action, Dictionary<string, object> queryData, ICommunicationModel body = null)
    {
        var queryString = httpClient.QueryStringFromDict(ToGodotDictionaryRecursive(queryData));
        var url = $"{serverUrl}/{action}?{queryString}";
        var stringBody = (body != null) ? CommunicationSerializer.SerializeObject(body) : null;
        var result = await this.SendRequest(url, stringBody);
        var response = Encoding.UTF8.GetString((byte[])result[3]);
        GD.Print($"Received response with code {(int)result[1]}: {response}");

        if ((int)result[1] != 200)
        {
            return new ClientResponse
            {
                result = (int)result[0],
                code = (int)result[1],
                headers = (string[])result[2],
                body = null
            };
        }

        return new ClientResponse
        {
            result = (int)result[0],
            code = (int)result[1],
            headers = (string[])result[2],
            body = CommunicationSerializer.DeserializeObject<ICommunicationModel>(response)
        };
    }

    public static Godot.Collections.Dictionary ToGodotDictionaryRecursive(IDictionary source)
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
