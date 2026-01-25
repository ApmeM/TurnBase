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

            var preparedField = Field2D.Create(model.Request.Width, model.Request.Height);
            for (var i = 0; i < model.Request.Width; i++)
            {
                for (var j = 0; j < model.Request.Height; j++)
                {
                    var ship = model.Request.AvailableFigures[r.Next(model.Request.AvailableFigures.Count)];
                    preparedField.trySet(i, j, new KaNoBuFigure(this.myNumber, ship));
                    model.Request.AvailableFigures.Remove(ship);
                }
            }

            return new InitResponseModel<KaNoBuInitResponseModel>(name, new KaNoBuInitResponseModel(preparedField));
        }

        public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
        {
            this.memorizedField.SynchronizeField(model.Request.Field);
            var from = this.findAllMovement(this.memorizedField.Field).OrderByDescending(a => EvaluateMove(this.memorizedField.Field, a)).ToList();

            if (from.Count == 0)
            {
                return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(new KaNoBuMoveResponseModel(KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN, default, default));
            }

            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(from[0]);
        }

        private int EvaluateMove(IField field, KaNoBuMoveResponseModel a)
        {
            var shipFrom = field.get(a.From.X, a.From.Y) as KaNoBuFigure;
            var shipTo = field.get(a.To.X, a.To.Y) as KaNoBuFigure;
            if (shipTo != null && shipTo.PlayerId != this.myNumber)
            {
                if (shipTo.FigureType == KaNoBuFigure.FigureTypes.Unknown)
                {
                    return 8; // Attack unknown enemy
                }
                if (shipTo.FigureType == KaNoBuRules.Looser[shipFrom.FigureType])
                {
                    return 10; // Attack loosing enemy
                }
                return -10; // Do not attack winning enemy
            }
            var enemyNearby = false;
            foreach (var dir in directions)
            {
                var to = new Point { X = a.To.X + dir.X, Y = a.To.Y + dir.Y };
                if (to.X < 0 || to.Y < 0 || to.X >= field.Width || to.Y >= field.Height)
                {
                    continue;
                }
                var shipNearby = field.get(to.X, to.Y) as KaNoBuFigure;
                if (shipNearby != null && shipNearby.PlayerId != this.myNumber)
                {
                    enemyNearby = true;
                }
            }
            if (enemyNearby)
            {
                return 5; // Prioritize moving from enemy
            }

            Point? myFlag = null;
            for (int x = 0; x < field.Width; x++)
            {
                for (int y = 0; y < field.Height; y++)
                {
                    var ship = field.get(x, y) as KaNoBuFigure;
                    if (ship != null && ship.PlayerId == this.myNumber && ship.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
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

        private IEnumerable<KaNoBuMoveResponseModel> findAllMovement(IField field)
        {
            for (int x = 0; x < field.Width; x++)
            {
                for (int y = 0; y < field.Height; y++)
                {
                    var from = new Point { X = x, Y = y };
                    var shipFrom = field.get(x, y) as KaNoBuFigure;
                    if (shipFrom == null)
                    {
                        continue;
                    }

                    if (shipFrom.PlayerId != this.myNumber)
                    {
                        continue;
                    }

                    if (shipFrom.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
                    {
                        continue;
                    }

                    foreach (var dir in directions)
                    {
                        var to = new Point { X = x + dir.X, Y = y + dir.Y };
                        if (to.X < 0 || to.Y < 0 || to.X >= field.Width || to.Y >= field.Height)
                        {
                            continue;
                        }

                        var shipTo = field.get(to.X, to.Y) as KaNoBuFigure;
                        if (shipTo == null || shipTo.PlayerId != this.myNumber)
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
            this.memorizedField.Clear();
        }

        public void PlayersInitialized()
        {
            this.memorizedField.Clear();
        }

        public void GameLogCurrentField(IField mainField)
        {
            this.memorizedField.SynchronizeField(mainField);
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