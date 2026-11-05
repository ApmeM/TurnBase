using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public override IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> CreateGame()
    {
        var victoryCondition = new ShipReachesColumnGameCondtion(6);
        var looseCondition = new NoMoveableFiguresGameCondition();
        var gameOverCondition = new AnyPlayerGameOverCondition(
            new AndGameCondition(
                new PlayerFilterGameCondtion(0),
                new OrGameCondition(victoryCondition, looseCondition)));

        var field = this.generateField();

        var rules = new KaNoBuRulesPredefinedFieldWithShips(field)
        {
            EnemyVisible = true,
            VictoryCondition = victoryCondition,
            LooseCondition = looseCondition,
            GameOverCondition = gameOverCondition
        };

        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>(rules, "level");
        game.AddPlayer(this);
        game.AddPlayer(new Level1Player());
        return game;
    }

    private sealed class Level1Player : IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
    {
        private readonly List<PatrolState> patrols = new List<PatrolState>();
        private int playerNumber;
        private int maxMovesPerTurn;

        public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(
            InitModel<KaNoBuInitModel> model,
            CancellationToken token = default)
        {
            this.playerNumber = model.PlayerId;
            this.maxMovesPerTurn = model.Request.MaxMovesPerTurn;
            this.patrols.Clear();

            var preparedField = PlayerInitHelper.CreateOrderedField(this.playerNumber, model.Request);

            return new InitResponseModel<KaNoBuInitResponseModel>
            {
                Name = "Level 1 Player",
                Response = new KaNoBuInitResponseModel(preparedField)
            };
        }

        public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(
            MakeTurnModel<KaNoBuMoveModel> model,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var field = model.Request.Field.Field;
            this.SynchronizePatrols(field);

            var moves = new List<KaNoBuMoveResponseModel.MoveStep>();
            var moveLimit = this.maxMovesPerTurn > 0 ? this.maxMovesPerTurn : 0;
            foreach (var patrol in this.patrols.OrderBy(state => state.CurrentPosition.Y).ThenBy(state => state.LaneX).ToList())
            {
                if (moves.Count >= moveLimit)
                {
                    break;
                }

                this.PlanPatrolMove(field, patrol, moves, moveLimit);
            }

            return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
            {
                Response = new KaNoBuMoveResponseModel(moves)
            };
        }

        public void GameStarted()
        {
            this.patrols.Clear();
        }

        public void GamePlayerInit(int playerNumber, string playerName)
        {
        }

        public void GamePlayersInitialized()
        {
        }

        public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification)
        {
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

        private void SynchronizePatrols(Field2D field)
        {
            var movableShips = new List<Point>();
            for (var x = 0; x < field.Width; x++)
            {
                for (var y = 0; y < field.Height; y++)
                {
                    var figure = field.GetFigure(x, y) as KaNoBuFigure;
                    if (figure != null && figure.PlayerId == this.playerNumber && figure.IsMoveable)
                    {
                        movableShips.Add(new Point(x, y));
                    }
                }
            }

            this.patrols.RemoveAll(state => !movableShips.Any(position => SamePoint(position, state.CurrentPosition)));
            foreach (var position in movableShips)
            {
                if (!this.patrols.Any(state => SamePoint(state.CurrentPosition, position)))
                {
                    this.patrols.Add(new PatrolState(position.X, position, position.Y == 0 ? 1 : -1));
                }
            }
        }

        private void PlanPatrolMove(
            Field2D field,
            PatrolState patrol,
            List<KaNoBuMoveResponseModel.MoveStep> moves,
            int moveLimit)
        {
            var current = field.GetFigure(patrol.CurrentPosition) as KaNoBuFigure;
            if (current == null || current.PlayerId != this.playerNumber || !current.IsMoveable)
            {
                patrol.IsAlive = false;
                return;
            }

            if (patrol.CurrentPosition.X != patrol.LaneX)
            {
                var returnToLane = new Point(patrol.LaneX, patrol.CurrentPosition.Y);
                if (this.CanMove(field, current, patrol.CurrentPosition, returnToLane, false))
                {
                    this.ExecuteMove(field, patrol, returnToLane, moves);
                }

                return;
            }

            var forward = new Point(patrol.CurrentPosition.X, patrol.CurrentPosition.Y + patrol.Direction);
            var adjacentEnemies = new[]
            {
            forward,
            new Point(patrol.CurrentPosition.X - 1, patrol.CurrentPosition.Y),
            new Point(patrol.CurrentPosition.X + 1, patrol.CurrentPosition.Y),
            new Point(patrol.CurrentPosition.X, patrol.CurrentPosition.Y - patrol.Direction)
        };

            foreach (var target in adjacentEnemies)
            {
                if (!IsEnemyAt(field, target, this.playerNumber) ||
                    !this.CanMove(field, current, patrol.CurrentPosition, target, true))
                {
                    continue;
                }

                this.ExecuteMove(field, patrol, target, moves);
                if (patrol.IsAlive && patrol.CurrentPosition.X != patrol.LaneX && moves.Count < moveLimit)
                {
                    var movedFigure = field.GetFigure(patrol.CurrentPosition) as KaNoBuFigure;
                    var returnToLane = new Point(patrol.LaneX, patrol.CurrentPosition.Y);
                    if (movedFigure != null && movedFigure.PlayerId == this.playerNumber &&
                        this.CanMove(field, movedFigure, patrol.CurrentPosition, returnToLane, false))
                    {
                        this.ExecuteMove(field, patrol, returnToLane, moves);
                    }
                }

                return;
            }

            if (!this.CanMove(field, current, patrol.CurrentPosition, forward, false))
            {
                patrol.Direction *= -1;
                forward = new Point(patrol.CurrentPosition.X, patrol.CurrentPosition.Y + patrol.Direction);
            }

            if (this.CanMove(field, current, patrol.CurrentPosition, forward, false))
            {
                this.ExecuteMove(field, patrol, forward, moves);
            }
        }

        private bool CanMove(Field2D field, KaNoBuFigure figure, Point from, Point to, bool allowEnemy)
        {
            if (!field.IsInBounds(to) || field.GetWall(to) ||
                !figure.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(from, to)))
            {
                return false;
            }

            var occupant = field.GetFigure(to) as KaNoBuFigure;
            return occupant == null || (allowEnemy && occupant.PlayerId != this.playerNumber);
        }

        private void ExecuteMove(
            Field2D field,
            PatrolState patrol,
            Point target,
            List<KaNoBuMoveResponseModel.MoveStep> moves)
        {
            var from = patrol.CurrentPosition;
            var attacker = field.GetFigure(from) as KaNoBuFigure;
            if (attacker == null)
            {
                patrol.IsAlive = false;
                return;
            }

            moves.Add(new KaNoBuMoveResponseModel.MoveStep(from, target));
            var defender = field.GetFigure(target) as KaNoBuFigure;
            if (defender == null)
            {
                field.SetFigure(target, attacker);
                field.SetFigure(from, null);
                patrol.CurrentPosition = target;
                return;
            }

            var resolution = attacker.ResolveBattle(defender);
            if (resolution.Outcome == KaNoBuMoveNotificationModel.BattleResult.BothDestroyed)
            {
                field.SetFigure(from, null);
                field.SetFigure(target, null);
                patrol.IsAlive = false;
                return;
            }

            var winner = resolution.Winner;
            if (winner == null)
            {
                return;
            }

            field.SetFigure(from, null);
            field.SetFigure(target, winner);
            if (winner.PlayerId != this.playerNumber)
            {
                patrol.IsAlive = false;
                return;
            }

            winner.WinNumber++;
            if (winner.WinNumber % 3 == 0)
            {
                winner = winner.WithFigureType(KaNoBuFigure.FigureTypes.ShipUniversal);
                field.SetFigure(target, winner);
            }

            if (resolution.Outcome == KaNoBuMoveNotificationModel.BattleResult.FlagCaptured)
            {
                for (var x = 0; x < field.Width; x++)
                {
                    for (var y = 0; y < field.Height; y++)
                    {
                        var figure = field.GetFigure(x, y) as KaNoBuFigure;
                        if (figure != null && figure.PlayerId == defender.PlayerId)
                        {
                            figure.PlayerId = this.playerNumber;
                        }
                    }
                }
            }

            patrol.CurrentPosition = target;
        }

        private static bool IsEnemyAt(Field2D field, Point position, int playerNumber)
        {
            return field.IsInBounds(position) &&
                field.GetFigure(position) is KaNoBuFigure figure &&
                figure.PlayerId != playerNumber;
        }

        private static bool SamePoint(Point first, Point second)
        {
            return first.X == second.X && first.Y == second.Y;
        }

        private sealed class PatrolState
        {
            public PatrolState(int laneX, Point currentPosition, int direction)
            {
                this.LaneX = laneX;
                this.CurrentPosition = currentPosition;
                this.Direction = direction;
                this.IsAlive = true;
            }

            public int LaneX { get; private set; }
            public Point CurrentPosition { get; set; }
            public int Direction { get; set; }
            public bool IsAlive { get; set; }
        }
    }
}
