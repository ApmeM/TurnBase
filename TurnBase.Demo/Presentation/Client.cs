using Godot;
using System.Threading.Tasks;

[SceneReference("Client.tscn")]
public partial class Client
{
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
}
