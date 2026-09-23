using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase
{
    public class PlayerLoose<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField> : 
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel, TField>
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

        public void GameLogCurrentField(TField field)
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

        public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model, CancellationToken token = default)
        {
            return new InitResponseModel<TInitResponseModel>();
        }

        public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model, CancellationToken token = default)
        {
            return new MakeTurnResponseModel<TMoveResponseModel>();
        }
    }
}