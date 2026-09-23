using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{
    public interface IGame
    {
        Task Play(CancellationToken token = default);
    }

    public interface IGame<
        TInitModel, 
        TInitResponseModel, 
        TMoveModel, 
        TMoveResponseModel, 
        TMoveNotificationModel, 
        TField> : IGame
    {
        string GameId { get; }

        AddPlayerStatus AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> player);

        void AddGameLogListener(IGameEventListener<TMoveNotificationModel, TField> gameLogListener);
        
        void Disconnect(IGameEventListener<TMoveNotificationModel, TField> player);
    }
}