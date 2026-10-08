using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{
    public interface IServer
    {
        void RegisterPlayer(string playerId, string gameId);
        void SendRequest(string playerId, ICommunicationModel model);
        Task<T> SendRequest<T>(string playerId, ICommunicationModel model, CancellationToken token = default);
    }
}
