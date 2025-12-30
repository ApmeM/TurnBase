using System.Threading.Tasks;
using TurnBase;

namespace TurnBase.KaNoBu
{
    public class KaNoBuPlayerLoose : IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>
    {
        public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
        {
            return new InitResponseModel<KaNoBuInitResponseModel>();
        }

        public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
        {
            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>();
        }
    }
}