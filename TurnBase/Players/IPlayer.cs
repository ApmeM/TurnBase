using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{
    public interface IPlayer
    {

    }

    public interface IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> : 
        IPlayer, 
        IGameEventListener<TMoveNotificationModel, TField>
    {
        #region Requests for actions
        Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model, CancellationToken token = default);
        Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model, CancellationToken token = default);
        #endregion
    }
}