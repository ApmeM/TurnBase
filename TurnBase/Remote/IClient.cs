using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{
    public interface IClient
    {
        Task<ClientResponse> SendAction(string serverUrl, string action, Dictionary<string, object> queryData, ICommunicationModel body, CancellationToken token);
    }
}