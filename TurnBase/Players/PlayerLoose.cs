using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurnBase
{
    public class PlayerLoose<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
    {
        public void GameFinished(List<int> winners)
        {
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
        }

        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
        }

        public void GameStarted()
        {
        }

        public void GameTurnFinished()
        {
        }

        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model)
        {
            return new InitResponseModel<TInitResponseModel>();
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model)
        {
            return new MakeTurnResponseModel<TMoveResponseModel>();
        }

        public void PlayersInitialized(IField mainField)
        {
        }
    }
}