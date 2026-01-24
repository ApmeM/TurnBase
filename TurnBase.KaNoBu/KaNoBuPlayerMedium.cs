using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class KaNoBuPlayerMedium :
        IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>,
        IGameEventListener<KaNoBuMoveNotificationModel>
    {
        private Random r = new Random();
        private string name = "Computer easy";
        private int myNumber;

        private KaNoBuMoveModel.FigureModel?[,] field;

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
            if (field == null)
            {
                field = model.Request.Field;
            }
            else
            {
                for (var x = 0; x < model.Request.Field.GetLength(0); x++)
                {
                    for (var y = 0; y < model.Request.Field.GetLength(1); y++)
                    {
                        var ship = model.Request.Field[x, y];
                        if (ship != null)
                        {
                            if (field[x, y] == null)
                            {
                                throw new Exception("Inconsistent field state");
                            }

                            field[x, y] = new KaNoBuMoveModel.FigureModel
                            {
                                PlayerNumber = ship.Value.PlayerNumber,
                                FigureType = field[x, y]?.FigureType == KaNoBuFigure.FigureTypes.Unknown ? ship.Value.FigureType : field[x, y].Value.FigureType
                            };
                        }
                    }
                }
            }

            var from = this.findAllMovement(model.Request.Field).OrderByDescending(a => EvaluateMove(this.field, a)).ToList();

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

        private List<Point> directions = new List<Point>
        {
            new Point { X = -1, Y = 0 },
            new Point { X = 1, Y = 0 },
            new Point { X = 0, Y = -1 },
            new Point { X = 0, Y = 1 }
        };

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
            if (this.field == null || notification.move.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
            {
                return;
            }

            var fromMapPos = notification.move.From;
            var toMapPos = notification.move.To;


            var movedUnit = this.field[fromMapPos.X, fromMapPos.Y].Value;

            if (notification.battle.HasValue)
            {
                var defenderUnit = this.field[toMapPos.X, toMapPos.Y].Value;

                switch (notification.battle.Value.battleResult)
                {
                    case KaNoBuMoveNotificationModel.BattleResult.Draw:
                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = movedUnit.FigureType;
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = defenderUnit.FigureType;
                        this.field[fromMapPos.X, fromMapPos.Y] = movedUnit;
                        this.field[toMapPos.X, toMapPos.Y] = defenderUnit;
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.AttackerWon:
                        // Attacker won

                        if (movedUnit.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
                        {
                            movedUnit.FigureType = KaNoBuFigure.FigureTypes.Unknown;
                        }
                        if (notification.battle.Value.isDefenderFlag)
                        {
                            defenderUnit.FigureType = KaNoBuFigure.FigureTypes.ShipFlag;
                        }
                        else
                        {
                            if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = KaNoBuRules.Looser[movedUnit.FigureType];
                            if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = KaNoBuRules.Winner[defenderUnit.FigureType];
                        }
                        this.field[fromMapPos.X, fromMapPos.Y] = null;
                        this.field[toMapPos.X, toMapPos.Y] = movedUnit;
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                        // Defender won
                        if (defenderUnit.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal)
                        {
                            defenderUnit.FigureType = KaNoBuFigure.FigureTypes.Unknown;
                        }

                        if (movedUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.FigureType = KaNoBuRules.Winner[movedUnit.FigureType];
                        if (defenderUnit.FigureType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.FigureType = KaNoBuRules.Looser[defenderUnit.FigureType];

                        this.field[fromMapPos.X, fromMapPos.Y] = null;
                        this.field[toMapPos.X, toMapPos.Y] = defenderUnit;
                        break;
                }
            }
            else
            {
                // No battle - swim here.
                this.field[fromMapPos.X, fromMapPos.Y] = null;
                this.field[toMapPos.X, toMapPos.Y] = movedUnit;
            }
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