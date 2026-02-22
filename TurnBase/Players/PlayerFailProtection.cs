using System;
using System.Threading.Tasks;

namespace TurnBase
{
    public class PlayerFailProtection<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> :
        FailProtectedListener<TMoveNotificationModel>,
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
    {
        public static ILogger logger = new ConsoleLogger();

        private readonly IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player;

        public PlayerFailProtection(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player)
            : base(player)
        {
            this.player = player;
        }

        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model)
        {
            try
            {
                return await this.player.Init(model);
            }
            catch (Exception e)
            {
                logger.Log($"Player initialization failed with exception: {e}");
                return new InitResponseModel<TInitResponseModel>();
            }
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model)
        {
            try
            {
                return await this.player.MakeTurn(model);
            }
            catch(Exception e)
            {
                logger.Log($"Player turn failed with exception: {e}");
                return new MakeTurnResponseModel<TMoveResponseModel>();
            }
        }
    }
}