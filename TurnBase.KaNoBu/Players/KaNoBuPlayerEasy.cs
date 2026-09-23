using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TurnBase.KaNoBu
{
    public class KaNoBuPlayerEasy : IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>
    {
        private Random r = new Random();
        private string name = "Computer easy";
        private int myNumber;
        private int maxMovesPerTurn = int.MaxValue;

        public void GameFinished(List<int> winners)
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

        public void GameLogCurrentField(Field2D field)
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
            this.maxMovesPerTurn = model.Request.MaxMovesPerTurn;

            var preparedField = Field2D.Create(model.Request.Width, model.Request.Height);
            for (var i = 0; i < model.Request.Width; i++)
            {
                for (var j = 0; j < model.Request.Height; j++)
                {
                    var p = new Point { X = i, Y = j };
                    var ship = model.Request.AvailableFigures[r.Next(model.Request.AvailableFigures.Count)];
                    preparedField[p] = KaNoBuFigure.Create(this.myNumber, ship, 0);
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
            var from = this.findAllMovement(model.Request.Field);

            if (from == null || from.Count == 0)
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
                var availableMoves = from.FindAll(move => !movedShips.Contains(move.from));
                if (availableMoves.Count == 0)
                {
                    break;
                }

                var movement = availableMoves[r.Next(availableMoves.Count)];
                selectedMoves.Add(new KaNoBuMoveResponseModel.MoveStep (movement.from, movement.to));
                movedShips.Add(movement.from);
            }

            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
            {
                Response = new KaNoBuMoveResponseModel(selectedMoves)
            };
        }

        private List<(Point from, Point to)> findAllMovement(Field2D field)
        {
            var availableShips = new List<(Point from, Point to)>();
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

                    this.tryAdd(availableShips, field, from, x - 1, y);
                    this.tryAdd(availableShips, field, from, x + 1, y);
                    this.tryAdd(availableShips, field, from, x, y - 1);
                    this.tryAdd(availableShips, field, from, x, y + 1);

                }
            }
            return availableShips;
        }

        private void tryAdd(List<(Point from, Point to)> availableShips, Field2D field, Point from, int x, int y)
        {
            var to = new Point { X = x, Y = y };
            if (!field.IsInBounds(to))
            {
                return;
            }

            if (field.walls[to.X, to.Y])
            {
                return;
            }

            var shipTo = field[to] as KaNoBuFigure;
            if (shipTo == null || shipTo.PlayerId != this.myNumber)
            {
                availableShips.Add((from, to));
            }
        }
    }
}