using System;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{
    public class PlayerFailProtection<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> :
        FailProtectedListener<TMoveNotificationModel, TField>,
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField>
    {
        public static ILogger logger = new ConsoleLogger();

        private readonly IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> player;

        public PlayerFailProtection(IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> player)
            : base(player)
        {
            this.player = player;
        }

        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model, CancellationToken token = default)
        {
            try
            {
                return await this.player.Init(model, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Log($"Player initialization failed with exception: {e}");
                return new InitResponseModel<TInitResponseModel>();
            }
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model, CancellationToken token = default)
        {
            try
            {
                return await this.player.MakeTurn(model, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch(Exception e)
            {
                logger.Log($"Player turn failed with exception: {e}");
                return new MakeTurnResponseModel<TMoveResponseModel>();
            }
        }
    }
}