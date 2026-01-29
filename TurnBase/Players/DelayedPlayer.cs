using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class DelayedPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : 
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
            int turnDelay)
        {
            this.player = originalPlayer;
            this.delayAction = delayAction;
            this.initDelay = initDelay;
            this.turnDelay = turnDelay;
        }

        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model)
        {
            await delayAction(this.initDelay);
            return await this.player.Init(model);
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model)
        {
            await delayAction(this.turnDelay);
            return await this.player.MakeTurn(model);
        }

        public void GameStarted()
        {
            this.player.GameStarted();
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
            this.player.GamePlayerInit(playerNumber, playerName);
        }

        public void PlayersInitialized()
        {
            this.player.PlayersInitialized();
        }
        
        public void GameLogCurrentField(IField field)
        {
            this.player.GameLogCurrentField(field);
        }
        
        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
            this.player.GamePlayerTurn(playerNumber, notification);
        }

        public void GameTurnFinished()
        {
            this.player.GameTurnFinished();
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
            this.player.GamePlayerDisconnected(playerNumber);
        }

        public void GameFinished(List<int> winners)
        {
            this.player.GameFinished(winners);
        }
    }
}