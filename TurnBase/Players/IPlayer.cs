using System.Threading.Tasks;

namespace TurnBase
{
    public interface IPlayer
    {

    }

    public interface IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> : IPlayer
    {
        Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model);

        Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model);
    }
}