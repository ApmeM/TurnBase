using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class DelayedPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> :
        PassThroughListener<TMoveNotificationModel>,
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
    {
        private IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> player;
        private readonly Func<int, Task> delayAction;
        private readonly int initDelay;
        private readonly int turnDelay;

        public DelayedPlayer(
            IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> originalPlayer,
            Func<int, Task> delayAction,
            int initDelay,
            int turnDelay) : base(originalPlayer)
        {
            this.player = originalPlayer;
            this.delayAction = delayAction;
            this.initDelay = initDelay;
            this.turnDelay = turnDelay;
        }

        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await delayAction(this.initDelay).WrapCancellation(token);
            token.ThrowIfCancellationRequested();
            return await this.player.Init(model, token);
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await delayAction(this.turnDelay).WrapCancellation(token);
            token.ThrowIfCancellationRequested();
            return await this.player.MakeTurn(model, token);
        }
    }
}