using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class KaNoBuPlayerNoMove : IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
    {
        private string name = "Player not moving";
        private int myNumber;

        public void GameFinished(List<int> winners)
        {
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
        }

        public void GamePlayersInitialized()
        {
        }

        public void GameLogCurrentField(KaNoBuField field)
        {
        }

        public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification)
        {
        }

        public void GameStarted()
        {
        }

        public void GameTurnFinished()
        {
        }

        public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model, CancellationToken token = default)
        {
            this.myNumber = model.PlayerId;

            var preparedField = Field2D.Create(model.Request.Width, model.Request.Height);
            for (var i = 0; i < model.Request.Width; i++)
            {
                for (var j = 0; j < model.Request.Height; j++)
                {
                    var p = new Point { X = i, Y = j };
                    var ship = model.Request.AvailableFigures[0];
                    preparedField.SetFigure(p, KaNoBuFigure.Create(this.myNumber, ship, 0));
                    model.Request.AvailableFigures.Remove(ship);
                }
            }

            return new InitResponseModel<KaNoBuInitResponseModel>
            {
                Name = name,
                Response = new KaNoBuInitResponseModel(preparedField)
            };
        }

        public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model, CancellationToken token = default)
        {
            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
            {
                Response = new KaNoBuMoveResponseModel(new List<KaNoBuMoveResponseModel.MoveStep>())
            };
        }
    }
}