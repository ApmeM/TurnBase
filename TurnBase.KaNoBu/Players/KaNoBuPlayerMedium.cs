using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class KaNoBuPlayerMedium :
        IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>
    {
        private Random r = new Random();
        private string name = "Computer medium";
        private int myNumber;
        private int maxMovesPerTurn = int.MaxValue;

        private List<Point> directions = new List<Point>
        {
            new Point { X = -1, Y = 0 },
            new Point { X = 1, Y = 0 },
            new Point { X = 0, Y = -1 },
            new Point { X = 0, Y = 1 }
        };

        private KaNoBuFieldMemorization memorizedField = new KaNoBuFieldMemorization();

        public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model, CancellationToken token = default)
        {
            this.myNumber = model.PlayerId;
            this.maxMovesPerTurn = model.Request.MaxMovesPerTurn;

            var preparedField = Field2D.Create(model.Request.Width, model.Request.Height);
            for (var i = 0; i < model.Request.Width; i++)
            {
                for (var j = 0; j < model.Request.Height; j++)
                {
                    var ship = model.Request.AvailableFigures[r.Next(model.Request.AvailableFigures.Count)];
                    preparedField[i, j] = KaNoBuFigure.Create(this.myNumber, ship, 0);
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
            this.memorizedField.SynchronizeField(model.Request.Field);
            var from = this.findAllMovement(this.memorizedField.Field)
                    .Select(move => (move.from, move.to, EvaluateMove(this.memorizedField.Field, move)))
                    .OrderByDescending(a => a.Item3)
                    .ToList();
            if (from.Count == 0)
            {
                return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
                {
                    Response = new KaNoBuMoveResponseModel(null)
                };
            }

            var selectedMoves = new List<KaNoBuMoveResponseModel.MoveStep>();
            var movedShips = new HashSet<Point>();
            while (selectedMoves.Count < this.maxMovesPerTurn)
            {
                var availableMoves = from
                    .Where(candidate => !movedShips.Contains(candidate.from))
                    .ToList();
                if (availableMoves.Count == 0)
                {
                    break;
                }

                var bestScore = availableMoves[0].Item3;
                var bestMoves = availableMoves.Where(candidate => candidate.Item3 == bestScore).ToList();
                var move = bestMoves[r.Next(bestMoves.Count)];
                selectedMoves.Add(new KaNoBuMoveResponseModel.MoveStep(move.from, move.to));
                movedShips.Add(move.from);
            }

            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
            {
                Response = new KaNoBuMoveResponseModel(selectedMoves)
            };
        }
        private int EvaluateMove(Field2D mainField, (Point from, Point to) move)
        {
            var from = move.from;
            var to = move.to;
            
            var field = (Field2D)mainField;
            var shipFrom = field[from] as KaNoBuFigure;
            var shipTo = field[to] as KaNoBuFigure;
            if (shipTo != null && shipTo.PlayerId != this.myNumber)
            {
                if (shipTo.FigureType == KaNoBuFigure.FigureTypes.Unknown)
                {
                    return 8; // Attack unknown enemy
                }
                if (shipTo.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
                {
                    return 100; // Attack flag enemy
                }

                if (shipFrom.FigureType == KaNoBuFigure.FigureTypes.ShipUniversal || 
                    (
                        shipTo.FigureType != KaNoBuFigure.FigureTypes.Unknown &&
                        shipFrom.ResolveBattle(shipTo).Outcome == KaNoBuMoveNotificationModel.BattleResult.AttackerWon)
                    )
                {
                    return 10; // Attack loosing enemy
                }
                return -10; // Do not attack winning enemy
            }

            var enemyNearby = false;
            foreach (var dir in directions)
            {
                var checkTo = new Point { X = to.X + dir.X, Y = to.Y + dir.Y };
                if (!field.IsInBounds(checkTo))
                {
                    continue;
                }
                var shipNearby = field[checkTo] as KaNoBuFigure;
                if (shipNearby != null && shipNearby.PlayerId != this.myNumber)
                {
                    enemyNearby = true;
                }
            }

            if (enemyNearby)
            {
                return 5; // Prioritize moving to enemy
            }

            Point? closestEnemy = null;
            for (int x = 0; x < field.Width; x++)
            {
                for (int y = 0; y < field.Height; y++)
                {
                    var p = new Point { X = x, Y = y };
                    var ship = field[p] as KaNoBuFigure;
                    if (ship != null && ship.PlayerId != this.myNumber && (ship.FigureType == KaNoBuFigure.FigureTypes.Unknown || ship.FigureType == KaNoBuFigure.FigureTypes.ShipFlag))
                    {
                        if (closestEnemy == null)
                        {
                            closestEnemy = p;
                        }
                        else
                        {
                            var dst = Math.Abs(closestEnemy.Value.X - from.X) + Math.Abs(closestEnemy.Value.Y - from.Y);
                            var newDst = Math.Abs(p.X - from.X) + Math.Abs(p.Y - from.Y);
                            if (newDst < dst)
                            {
                                closestEnemy = p;
                            }
                        }
                    }
                }
            }
            if (closestEnemy == null)
            {
                return 0;
            }

            {
                var dst = Math.Abs(closestEnemy.Value.X - from.X) + Math.Abs(closestEnemy.Value.Y - from.Y);
                var newDst = Math.Abs(closestEnemy.Value.X - to.X) + Math.Abs(closestEnemy.Value.Y - to.Y);
                return dst > newDst ? 6 : -6; // Prioritize moving to closest Unknown or Flag enemy
            }
        }

        private IEnumerable<(Point from, Point to)> findAllMovement(Field2D mainField)
        {
            var field = (Field2D)mainField;
            for (int x = 0; x < field.Width; x++)
            {
                for (int y = 0; y < field.Height; y++)
                {
                    var from = new Point { X = x, Y = y };
                    var shipFrom = field[from] as KaNoBuFigure;
                    if (shipFrom == null)
                    {
                        continue;
                    }

                    if (shipFrom.PlayerId != this.myNumber)
                    {
                        continue;
                    }

                    if (shipFrom.FigureType == KaNoBuFigure.FigureTypes.ShipFlag || shipFrom.FigureType == KaNoBuFigure.FigureTypes.ShipMine)
                    {
                        continue;
                    }

                    foreach (var dir in directions)
                    {
                        var to = new Point { X = x + dir.X, Y = y + dir.Y };
                        if (!field.IsInBounds(to))
                        {
                            continue;
                        }

                        if (field.walls[to.X, to.Y])
                        {
                            continue;
                        }

                        var shipTo = field[to] as KaNoBuFigure;
                        if (shipTo == null || shipTo.PlayerId != this.myNumber)
                        {
                            yield return (from, to);
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

        public void GameLogCurrentField(Field2D mainField)
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