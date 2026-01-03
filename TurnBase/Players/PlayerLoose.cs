using System.Threading.Tasks;

namespace TurnBase
{
    public class PlayerLoose<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> : IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>
    {
        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model)
        {
            return new InitResponseModel<TInitResponseModel>();
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model)
        {
            return new MakeTurnResponseModel<TMoveResponseModel>();
        }
    }
}