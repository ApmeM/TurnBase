using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class KaNoBuPlayerEasy : IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
    {
        private Random r = new Random();
        private string name = "Computer easy";
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

        public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification)
        {
        }

        public void GameStarted()
        {
        }

        public void GameTurnFinished()
        {
        }

        public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
        {
            this.myNumber = model.PlayerId;

            var preparedField = new KaNoBuFigure.FigureTypes[model.Request.Width,model.Request.Height];
            for (var i = 0; i < model.Request.Width; i++)
            {
                for (var j = 0; j < model.Request.Height; j++)
                {
                    var ship = model.Request.AvailableFigures[r.Next(model.Request.AvailableFigures.Count)];
                    preparedField[i,j] = ship; 
                    model.Request.AvailableFigures.Remove(ship);
                }
            }

            return new InitResponseModel<KaNoBuInitResponseModel>(name, new KaNoBuInitResponseModel(preparedField));
        }

        public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
        {
            var from = this.findAllMovement(model.Request.Field);

            if (from == null || from.Count == 0)
            {
                return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN, default, default));
            }

            int movementNum = r.Next(from.Count);

            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(from[movementNum]);
        }

        private List<KaNoBuMoveResponseModel> findAllMovement(KaNoBuMoveModel.FigureModel?[,] field)
        {
            var availableShips = new List<KaNoBuMoveResponseModel>();
            for (int x = 0; x < field.GetLength(0); x++)
            {
                for (int y = 0; y < field.GetLength(1); y++)
                {
                    var from = new Point { X = x, Y = y };
                    var shipFrom = field[x,y];
                    if (shipFrom == null)
                    {
                        continue;
                    }

                    if (shipFrom.Value.PlayerNumber != this.myNumber)
                    {
                        continue;
                    }

                    if (shipFrom.Value.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
                    {
                        continue;
                    }

                    this.tryAdd(availableShips, field, from, x - 1, y);
                    this.tryAdd(availableShips, field, from, x + 1, y);
                    this.tryAdd(availableShips, field, from, x, y - 1);
                    this.tryAdd(availableShips, field, from, x, y + 1);

                }
            }
            return availableShips;
        }

        private void tryAdd(List<KaNoBuMoveResponseModel> availableShips, KaNoBuMoveModel.FigureModel?[,] field, Point from, int x, int y)
        {
            if (x < 0 || y < 0 || x >= field.GetLength(0) || y >= field.GetLength(1))
            {
                return;
            }

            var to = new Point { X = x, Y = y };
            var shipTo = field[x,y];
            if (shipTo == null || shipTo.Value.PlayerNumber != this.myNumber)
            {
                availableShips.Add(new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN, from, to));
            }
        }
    }
}