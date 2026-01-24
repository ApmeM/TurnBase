using System.Threading.Tasks;

namespace TurnBase
{
    public interface IPlayer
    {

    }

    public interface IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : 
        IPlayer, 
        IGameEventListener<TMoveNotificationModel>
    {
        #region Requests for actions
        Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model);
        Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model);
        #endregion
    }
}