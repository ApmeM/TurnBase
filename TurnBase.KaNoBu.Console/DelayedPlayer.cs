using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class DelayedPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> : IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>
    {
        private IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> player;
        private readonly int initDelay;
        private readonly int turnDelay;

        public DelayedPlayer(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel> originalPlayer, int initDelay, int turnDelay)
        {
            this.player = originalPlayer;
            this.initDelay = initDelay;
            this.turnDelay = turnDelay;
        }

        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model)
        {
            await Task.Delay(this.initDelay);
            return await this.player.Init(model);
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model)
        {
            await Task.Delay(this.turnDelay);
            return await this.player.MakeTurn(model);
        }
    }
}