using System.Threading.Tasks;

namespace TurnBase
{
    public interface IGame
    {
        Task Play();
    }

    public interface IGame<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : IGame
    {
        string GameId { get; }

        AddPlayerStatus AddPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player);

        void AddGameLogListener(IGameEventListener<TMoveNotificationModel> gameLogListener);
        
        void Disconnect(IGameEventListener<TMoveNotificationModel> player);
    }
}