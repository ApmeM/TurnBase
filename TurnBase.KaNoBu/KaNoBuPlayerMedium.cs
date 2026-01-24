using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class KaNoBuPlayerMedium :
        IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
    {
        private Random r = new Random();
        private string name = "Computer easy";
        private int myNumber;

        private List<Point> directions = new List<Point>
        {
            new Point { X = -1, Y = 0 },
            new Point { X = 1, Y = 0 },
            new Point { X = 0, Y = -1 },
            new Point { X = 0, Y = 1 }
        };

        private KaNoBuFieldMemorization memorizedField = new KaNoBuFieldMemorization();

        public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
        {
            this.myNumber = model.PlayerId;

            var preparedField = new KaNoBuFigure.FigureTypes[model.Request.Width, model.Request.Height];
            for (var i = 0; i < model.Request.Width; i++)
            {
                for (var j = 0; j < model.Request.Height; j++)
                {
                    var ship = model.Request.AvailableFigures[r.Next(model.Request.AvailableFigures.Count)];
                    preparedField[i, j] = ship;
                    model.Request.AvailableFigures.Remove(ship);
                }
            }

            return new InitResponseModel<KaNoBuInitResponseModel>(name, new KaNoBuInitResponseModel(preparedField));
        }

        public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
        {
            this.memorizedField.SynchronizeField(model);
            var from = this.findAllMovement(this.memorizedField.Field).OrderByDescending(a => EvaluateMove(this.memorizedField.Field, a)).ToList();

            if (from.Count == 0)
            {
                return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN, default, default));
            }

            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(from[0]);
        }

        private int EvaluateMove(KaNoBuMoveModel.FigureModel?[,] field, KaNoBuMoveResponseModel a)
        {
            var shipFrom = field[a.From.X, a.From.Y];
            var shipTo = field[a.To.X, a.To.Y];
            if (shipTo != null && shipTo.Value.PlayerNumber != this.myNumber)
            {
                if (shipTo.Value.FigureType == KaNoBuFigure.FigureTypes.Unknown)
                {
                    return 8; // Attack unknown enemy
                }
                if (shipTo.Value.FigureType == KaNoBuRules.Looser[shipFrom.Value.FigureType])
                {
                    return 10; // Attack loosing enemy
                }
                return -10; // Do not attack winning enemy
            }
            var enemyNearby = false;
            foreach (var dir in directions)
            {
                var to = new Point { X = a.To.X + dir.X, Y = a.To.Y + dir.Y };
                if (to.X < 0 || to.Y < 0 || to.X >= field.GetLength(0) || to.Y >= field.GetLength(1))
                {
                    continue;
                }
                var shipNearby = field[to.X, to.Y];
                if (shipNearby != null && shipNearby.Value.PlayerNumber != this.myNumber)
                {
                    enemyNearby = true;
                }
            }
            if (enemyNearby)
            {
                return 5; // Prioritize moving from enemy
            }

            Point? myFlag = null;
            for (int x = 0; x < field.GetLength(0); x++)
            {
                for (int y = 0; y < field.GetLength(1); y++)
                {
                    var ship = field[x, y];
                    if (ship != null && ship.Value.PlayerNumber == this.myNumber && ship.Value.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
                    {
                        myFlag = new Point { X = x, Y = y };
                    }
                }
            }

            if (myFlag.HasValue)
            {
                var distBefore = Math.Abs(a.From.X - myFlag.Value.X) + Math.Abs(a.From.Y - myFlag.Value.Y);
                var distAfter = Math.Abs(a.To.X - myFlag.Value.X) + Math.Abs(a.To.Y - myFlag.Value.Y);
                return distBefore > distAfter ? -3 : 3; // Prioritize moving from flag
            }

            return 0;
        }

        private IEnumerable<KaNoBuMoveResponseModel> findAllMovement(KaNoBuMoveModel.FigureModel?[,] field)
        {
            for (int x = 0; x < field.GetLength(0); x++)
            {
                for (int y = 0; y < field.GetLength(1); y++)
                {
                    var from = new Point { X = x, Y = y };
                    var shipFrom = field[x, y];
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

                    foreach (var dir in directions)
                    {
                        var to = new Point { X = x + dir.X, Y = y + dir.Y };
                        if (to.X < 0 || to.Y < 0 || to.X >= field.GetLength(0) || to.Y >= field.GetLength(1))
                        {
                            continue;
                        }

                        var shipTo = field[to.X, to.Y];
                        if (shipTo == null || shipTo.Value.PlayerNumber != this.myNumber)
                        {
                            yield return new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN, from, to);
                        }
                    }
                }
            }
        }

        public void GameStarted()
        {
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
        }

        public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification)
        {
            this.memorizedField.UpdateKnownShips(notification);
        }

        public void GameTurnFinished()
        {
        }

        public void GamePlayerDisconnected(int playerNumber)
        {
        }

        public void GameFinished(List<int> winners)
        {
        }
    }
}