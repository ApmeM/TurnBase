using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurnBase
{
    public class PlayerLoose<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> : IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
    {

        public void GameStarted()
        {
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
        }

        public void PlayersInitialized()
        {
        }

        public void GameLogCurrentField(IField field)
        {
        }

        public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
        {
        }
        
        public void GameTurnFinished()
        {
        }

        public void GameFinished(List<int> winners)
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
    }
}