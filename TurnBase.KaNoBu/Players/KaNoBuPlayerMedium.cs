using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class KaNoBuPlayerMedium :
        IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
    {
        private Random r = new Random();
        private string name = "Computer medium";
        private int myNumber;
        private int maxMovesPerTurn = int.MaxValue;

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
                    preparedField.SetFigure(i, j, KaNoBuFigure.Create(this.myNumber, ship, 0));
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
            this.memorizedField.SynchronizeField(model.Request.Field.Field);
            var from = this.findAllMovement(this.memorizedField.Field)
                    .Select(move => (move.from, move.to, EvaluateMove(this.memorizedField.Field, move)))
                    .Where(move => move.Item3 > 0)
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

            var field = mainField;
            var shipFrom = field.GetFigure(from) as KaNoBuFigure;
            var shipTo = field.GetFigure(to) as KaNoBuFigure;
            if (shipTo != null && shipTo.PlayerId != this.myNumber)
            {
                if (shipTo.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
                {
                    return 100; // Attack flag enemy
                }

                if (shipTo.FigureType == KaNoBuFigure.FigureTypes.Unknown)
                {
                    return shipFrom.FigureType == KaNoBuFigure.FigureTypes.ShipScout
                        ? 80 // Scout discovers an unknown figure.
                        : 70; // Other ships attack an unknown figure.
                }

                return shipFrom.ResolveBattle(shipTo).Outcome == KaNoBuMoveNotificationModel.BattleResult.AttackerWon
                    ? 90 // Attack only when this ship is guaranteed to win.
                    : 0; // Do not select known fights that are lost, drawn, or mutually destructive.
            }

            if (shipFrom.FigureType == KaNoBuFigure.FigureTypes.ShipScout)
            {
                var adjacentEnemies = GetAdjacentEnemies(field, from);
                foreach (var enemyPosition in adjacentEnemies)
                {
                    if (to.ManhattanDistanceTo(enemyPosition) > from.ManhattanDistanceTo(enemyPosition))
                    {
                        return 60; // Scouts retreat from any adjacent enemy, regardless of type.
                    }
                }
            }
            else
            {
                // ToDo: Change throw exception to return IncorrectRequest outcome instead.
                var adjacentEnemiesThatCanWin = GetAdjacentEnemies(field, from)
                    .Where(a => ((KaNoBuFigure)field.GetFigure(a)).FigureType != KaNoBuFigure.FigureTypes.Unknown)
                    .Where(a => ((KaNoBuFigure)field.GetFigure(a)).FigureType != KaNoBuFigure.FigureTypes.ShipFlag)
                    .Where(a => ((KaNoBuFigure)field.GetFigure(a)).IsMoveable)
                    .Where(a => ((KaNoBuFigure)field.GetFigure(a)).ResolveBattle(shipFrom).Outcome == KaNoBuMoveNotificationModel.BattleResult.AttackerWon);

                foreach (var enemyPosition in adjacentEnemiesThatCanWin)
                {
                    if (to.ManhattanDistanceTo(enemyPosition) > from.ManhattanDistanceTo(enemyPosition))
                    {
                        return 60; // Retreat from an adjacent known enemy that would win.
                    }
                }
            }

            var closestViableEnemy = GetClosestViableEnemy(field, from);
            if (closestViableEnemy.HasValue &&
                to.ManhattanDistanceTo(closestViableEnemy.Value) < from.ManhattanDistanceTo(closestViableEnemy.Value))
            {
                return 50; // Moving towards closest viable enemy.
            }

            return 20; // Any other legal move.
        }

        private Point? GetClosestViableEnemy(Field2D field, Point from)
        {
            var shipFrom = field.GetFigure(from) as KaNoBuFigure;
            Point? closestViableEnemy = null;
            var closestDistance = int.MaxValue;
            for (var x = 0; x < field.Width; x++)
            {
                for (var y = 0; y < field.Height; y++)
                {
                    var enemy = field.GetFigure(x, y) as KaNoBuFigure;
                    if (enemy == null || enemy.PlayerId == this.myNumber)
                    {
                        continue;
                    }

                    var canApproach = enemy.FigureType == KaNoBuFigure.FigureTypes.Unknown ||
                        shipFrom.ResolveBattle(enemy).Outcome == KaNoBuMoveNotificationModel.BattleResult.AttackerWon;
                    var distance = from.ManhattanDistanceTo(new Point(x, y));
                    if (canApproach && distance < closestDistance)
                    {
                        closestViableEnemy = new Point(x, y);
                        closestDistance = distance;
                    }
                }
            }
            return closestViableEnemy;
        }

        private IEnumerable<Point> GetAdjacentEnemies(Field2D field, Point position)
        {
            return new[]{
                    new Point { X = -1, Y = 0 },
                    new Point { X = 1, Y = 0 },
                    new Point { X = 0, Y = -1 },
                    new Point { X = 0, Y = 1 }}
                .Select(a => position + a)
                .Where(a => field.IsInBounds(a))
                .Where(a => field.GetFigure(a) as KaNoBuFigure != null)
                .Where(a => field.GetFigure(a).PlayerId != this.myNumber);
        }

        private IEnumerable<(Point from, Point to)> findAllMovement(Field2D mainField)
        {
            var field = mainField;
            for (int x = 0; x < field.Width; x++)
            {
                for (int y = 0; y < field.Height; y++)
                {
                    var from = new Point { X = x, Y = y };
                    var shipFrom = field.GetFigure(from) as KaNoBuFigure;
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

                    var moveOffsets = shipFrom.GetPossibleMoveOffsets();
                    foreach (var dir in moveOffsets)
                    {
                        var to = new Point { X = x + dir.X, Y = y + dir.Y };
                        if (!field.IsInBounds(to))
                        {
                            continue;
                        }

                        if (field.GetWall(to))
                        {
                            continue;
                        }

                        var shipTo = field.GetFigure(to) as KaNoBuFigure;
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

        public void GamePlayersInitialized()
        {
            this.memorizedField.Clear();
        }

        public void GameLogCurrentField(KaNoBuField mainField)
        {
            this.memorizedField.SynchronizeField(mainField.Field);
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