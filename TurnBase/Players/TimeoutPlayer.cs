using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class TimeoutPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> :
        PassThroughListener<TMoveNotificationModel, TField>,
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField>
    {
        private IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> player;
        private readonly Func<int, Task> delayAction;
        private readonly int initDelay;
        private readonly int turnDelay;

        public TimeoutPlayer(
            IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> originalPlayer,
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
            var task1 = delayAction(this.initDelay);
            var task2 = this.player.Init(model, token);
            var task = await Task.WhenAny(task1, task2).WrapCancellation(token);
            if (task == task2)
            {
                return await task2;
            }

            return new InitResponseModel<TInitResponseModel>();
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var task1 = delayAction(this.turnDelay);
            var task2 = this.player.MakeTurn(model, token);
            var task = await Task.WhenAny(task1, task2).WrapCancellation(token);
            if (task == task2)
            {
                return await task2;
            }
            
            return new MakeTurnResponseModel<TMoveResponseModel>();
        }
    }
}