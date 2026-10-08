using System.Text;

namespace TurnBase.KaNoBu;

public sealed class ConsoleClient : IClient
{
    private static readonly HttpClient HttpClient = new();

    public async Task<ClientResponse> SendAction(
        string serverUrl,
        string action,
        Dictionary<string, object> queryData,
        ICommunicationModel body,
        CancellationToken token)
    {
        var query = string.Join("&", queryData.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(Convert.ToString(pair.Value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty)}"));
        var requestUri = $"{serverUrl.TrimEnd('/')}/{action}?{query}";
        using var request = new HttpRequestMessage(body == null ? HttpMethod.Get : HttpMethod.Post, requestUri);
        if (body != null)
        {
            request.Content = new StringContent(CommunicationSerializer.SerializeObject(body), Encoding.UTF8, "application/json");
        }

        using var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        return new ClientResponse
        {
            result = (int)response.StatusCode,
            code = (int)response.StatusCode,
            headers = response.Headers.Select(header => $"{header.Key}: {string.Join(", ", header.Value)}").ToArray(),
            body = response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(responseBody)
                ? CommunicationSerializer.DeserializeObject<ICommunicationModel>(responseBody)
                : null
        };
    }
}