using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("GameField.tscn")]
public partial class GameField :
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>
{
    [Export]
    public PackedScene UnitScene;
    [Export]
    public PackedScene GameInitScene;
    public int playerId { get; private set; } = -1;
    private int maxMovesPerTurn = int.MaxValue;
    public List<int> Winners { get; private set; }
    private KaNoBuFieldMemorization memorizedField = new KaNoBuFieldMemorization();
    private readonly List<Unit> movePreviews = new List<Unit>();
    public IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D> Game;

    public TileMap Water => this.water;

    #region IPlayer region

    public async Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model, CancellationToken token = default)
    {
        this.moveButtons.Visible = false;

        var gameInit = this.GameInitScene.Instance();
        this.AddChild(gameInit);

        this.playerId = model.PlayerId;
        this.maxMovesPerTurn = model.Request.MaxMovesPerTurn;
        _ = MoveCameraToPlayer();

        var result = await ((IGameInit)gameInit).Run(model, token).WrapCancellation(token);

        this.RemoveChild(gameInit);
        return result;        
    }

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model, CancellationToken token = default)
    {
        this.moveButtons.Visible = true;

        this.timerLabel.ShowMessage("Your turn", 1f);
        this.memorizedField.SynchronizeField((Field2D)model.Request.Field);
        this.UpdateKnownShips();
        var pendingTurnMoves = new List<KaNoBuMoveResponseModel.MoveStep>();
        this.moveButtons.Visible = true;
        this.UpdateMoveButtons(pendingTurnMoves.Count);

        while (true)
        {
            var moveTask = this.ToMySignal<Vector2, Vector2>(nameof(MoveDone)).WrapCancellation(token);
            var confirmTask = this.ToMySignal(nameof(TurnConfirmed)).WrapCancellation(token);
            var cancelTask = this.ToMySignal(nameof(TurnCancelled)).WrapCancellation(token);
            var winner = await Task.WhenAny(moveTask, confirmTask, cancelTask);

            if (winner == cancelTask)
            {
                pendingTurnMoves.Clear();
                this.ClearSelection();
                this.ClearMovePreviews();
                this.UpdateMoveButtons(pendingTurnMoves.Count);
                this.timerLabel.ShowMessage("Move cancelled. Start again.", 1.5f);
                continue;
            }

            if (winner == confirmTask)
            {
                if (pendingTurnMoves.Count == 0)
                {
                    continue;
                }

                this.moveButtons.Visible = false;
                this.ClearMovePreviews();
                var response = new KaNoBuMoveResponseModel(pendingTurnMoves);
                return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
                {
                    Response = response
                };
            }

            var (from, to) = await moveTask;
            if (from == to)
            {
                continue;
            }

            var movingUnit = this.field.GetChildren()
                .Cast<Unit>()
                .FirstOrDefault(a => a.TargetPositionMap == from && a.PlayerNumber == this.playerId);
            if (movingUnit == null || !movingUnit.GetPossibleMoves().Contains(to - from))
            {
                continue;
            }

            if (this.maxMovesPerTurn > 0 && pendingTurnMoves.Count >= this.maxMovesPerTurn)
            {
                this.timerLabel.ShowMessage($"Only {this.maxMovesPerTurn} move(s) can be made per turn.", 1.2f);
                continue;
            }

            pendingTurnMoves.Add(new KaNoBuMoveResponseModel.MoveStep(
                new Point { X = (int)from.x, Y = (int)from.y },
                new Point { X = (int)to.x, Y = (int)to.y }
            ));
            this.ShowMovePreview(from, to);
            this.UpdateMoveButtons(pendingTurnMoves.Count);
            this.timerLabel.ShowMessage($"Queued {pendingTurnMoves.Count} move(s).", 1.2f);
        }
    }

    #endregion

    public async Task MoveCameraToPlayer()
    {
    }

    public async Task MoveCameraToCenter()
    {
    }

    #region IGameEventListener region

    public void GameStarted()
    {
        this.playerId = -1;

        this.field.RemoveChildren();
        this.memorizedField.Clear();

        this.timerLabel.ShowMessage("Game Started.", 1f);
    }

    public void GamePlayerInit(int playerNumber, string playerName)
    {
        this.field.RemoveChildren();
        this.memorizedField.Clear();
    }

    public void PlayersInitialized()
    {
        this.memorizedField.Clear();
        this.field.RemoveChildren();
    }

    public virtual void GameLogCurrentField(Field2D mainField)
    {
        GD.Print("GameLogCurrentField:\n", mainField.ToString());
        this.memorizedField.SynchronizeField(mainField);
        if (this.field.GetChildCount() == 0)
        {
            for (var x = 0; x < mainField.Width; x++)
            {
                for (var y = 0; y < mainField.Height; y++)
                {
                    var pos = new Vector2(x, y);

                    var originalShip = mainField[x, y] as KaNoBuFigure;
                    if (originalShip == null)
                    {
                        continue;
                    }

                    var mapPos = new Vector2(x, y);
                    var worldPos = this.field.MapToWorld(mapPos);
                    var unit = (Unit)UnitScene.Instance();

                    unit.MoveUnitToLogic(mapPos);

                    unit.Position = worldPos + this.field.CellSize / 2;
                    unit.Connect(nameof(Unit.UnitClicked), this, nameof(OnUnitClicked), new Godot.Collections.Array { unit });

                    this.field.AddChild(unit);
                }
            }
        }

        this.UpdateKnownShips();
    }

    public async void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel turnNotification)
    {
        if (turnNotification.MoveNotifications.Count == 0)
        {
            return;
        }

        foreach (var notification in turnNotification.MoveNotifications)
        {
            var fromMapPos = new Vector2(notification.From.X, notification.From.Y);
            var toMapPos = new Vector2(notification.To.X, notification.To.Y);
            var toWorldPos = this.field.MapToWorld(toMapPos) + this.field.CellSize / 2;
            var allUnits = this.field.GetChildren();
            var movedUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == fromMapPos && a.PlayerNumber == playerNumber);
            if (notification.Battle.HasValue)
            {
                var defenderUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == toMapPos);
                switch (notification.Battle.Value.battleResult)
                {
                    case KaNoBuMoveNotificationModel.BattleResult.Draw:
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.BothDestroyed:
                        movedUnit.UnitHitLogic();
                        defenderUnit.UnitHitLogic();
                        movedUnit.AttackLogic();
                        movedUnit.CallbackAnimation((unit) => movedUnit.RotateUnitToAnimation(toWorldPos));
                        movedUnit.CallbackAnimation((unit) => movedUnit.AttackAnimation());
                        movedUnit.CallbackAnimation((unit) => movedUnit.UnitHitAnimation());
                        movedUnit.CallbackAnimation((unit) => defenderUnit.UnitHitAnimation());
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.AttackerWon:
                        // Attacker won
                        defenderUnit.UnitHitLogic();
                        movedUnit.MoveUnitToLogic(toMapPos);
                        movedUnit.AttackLogic();
                        movedUnit.CallbackAnimation((unit) => movedUnit.RotateUnitToAnimation(toWorldPos));
                        movedUnit.CallbackAnimation((unit) => movedUnit.AttackAnimation());
                        movedUnit.CallbackAnimation((unit) => defenderUnit.UnitHitAnimation());
                        movedUnit.CallbackAnimation((unit) => movedUnit.MoveUnitToAnimation(toWorldPos));
                        break;
                    case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                        // Defender won
                        movedUnit.UnitHitLogic();
                        defenderUnit.AttackLogic();
                        defenderUnit.CallbackAnimation((unit) => defenderUnit.RotateUnitToAnimation(movedUnit.Position));
                        defenderUnit.CallbackAnimation((unit) => defenderUnit.AttackAnimation());
                        defenderUnit.CallbackAnimation((unit) => movedUnit.UnitHitAnimation());
                        break;
                    default:
                        throw new Exception($"Unsupported battle result {notification.Battle.Value.battleResult}");
                }
            }
            else
            {
                // No battle - swim here.
                movedUnit.MoveUnitToLogic(toMapPos);
                movedUnit.CallbackAnimation((unit) => movedUnit.RotateUnitToAnimation(toWorldPos));
                movedUnit.CallbackAnimation((unit) => movedUnit.MoveUnitToAnimation(toWorldPos));
            }
        }

        this.memorizedField.UpdateKnownShips(turnNotification);
        this.UpdateKnownShips();
    }

    public void GameTurnFinished()
    {
    }

    public async void GameFinished(List<int> winners)
    {
        if (winners.Count > 1)
        {
            throw new Exception($"Unexpected number of winners : {winners.Count}.");
        }

        this.Winners = winners;

        _ = MoveCameraToCenter();
    }

    public void GamePlayerDisconnected(int playerNumber)
    {
        this.timerLabel.ShowMessage($"Player {playerNumber} disconnected.", 5);
    }

    #endregion

    private void UpdateKnownShips()
    {
        var allUnits = this.field.GetChildren();
        foreach (Unit unit in allUnits)
        {
            if (unit.TargetPositionMap == null)
            {
                continue;
            }
            var p = new Point((int)unit.TargetPositionMap.Value.x, (int)unit.TargetPositionMap.Value.y);
            var figure = this.memorizedField.Field[p] as KaNoBuFigure;
            unit.PlayerNumber = figure.PlayerId;
            unit.UnitType = figure.FigureType;
            unit.IsClickable = figure.PlayerId == this.playerId;
        }
    }

    [Signal]
    public delegate void MoveDone(Vector2 mapFrom, Vector2 mapTo);

    [Signal]
    public delegate void TurnConfirmed();

    [Signal]
    public delegate void TurnCancelled();

    private void ShowSelection(Unit unit)
    {
        var moves = unit.GetPossibleMoves();
        foreach (var move in moves)
        {
            var newPos = unit.TargetPositionMap.Value + move;
            if (this.field.GetCellv(newPos) == 4)
            {
                this.field.SetCellv(newPos, 5);
            }
        }
    }

    private void ClearSelection()
    {
        this.field.GetUsedCells()
                   .Cast<Vector2>()
                   .Select(point => (point, this.field.GetCellv(point)))
                   .Where(a => a.Item2 == 5)
                   .ToList()
                   .ForEach(p => this.field.SetCellv(p.point, 4));
        this.GetTree().GetNodesInGroup(Groups.IsSelected)
            .Cast<Unit>()
            .ToList()
            .ForEach(a =>
            {
                a.IsSelected = false;
                a.RemoveFromGroup(Groups.IsSelected);
            });
    }

    private async void ShowMovePreview(Vector2 fromMapPos, Vector2 toMapPos)
    {
        var unit = this.field.GetChildren()
            .Cast<Unit>()
            .FirstOrDefault(a => a.TargetPositionMap == fromMapPos && a.PlayerNumber == this.playerId);
        if (unit == null)
        {
            return;
        }

        var preview = this.UnitScene.Instance<Unit>();
        this.field.AddChild(preview);
        this.movePreviews.Add(preview);
        preview.PlayerNumber = unit.PlayerNumber;
        preview.UnitType = unit.UnitType;
        preview.IsClickable = false;
        preview.Modulate = new Color(1f, 1f, 1f, 0.35f);
        preview.ZIndex = 10;
        preview.Position = unit.Position;
        await preview.RotateUnitToAnimation(this.field.MapToWorld(toMapPos) + this.field.CellSize / 2);
        await preview.MoveUnitToAnimation(this.field.MapToWorld(toMapPos) + this.field.CellSize / 2);
    }

    private void ClearMovePreviews()
    {
        foreach (var preview in this.movePreviews)
        {
            if (Godot.Object.IsInstanceValid(preview))
            {
                preview.QueueFree();
            }
        }

        this.movePreviews.Clear();
    }

    private async void OnUnitClicked(Unit unit)
    {
        this.ClearSelection();
        this.ShowSelection(unit);

        var drag = this.drag;
        drag.StartDragging();

        var dragRes = await this.drag.ToSignal(this.drag, nameof(DragControl.DragFinished));
        var from = this.field.WorldToMap(this.field.ToLocal((Vector2)dragRes[0]));
        var to = this.field.WorldToMap(this.field.ToLocal((Vector2)dragRes[1]));

        if (from != to)
        {
            this.ClearSelection();
            this.EmitSignal(nameof(MoveDone), from, to);
        }
        else
        {
            unit.IsSelected = true;
            unit.AddToGroup(Groups.IsSelected);
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.AddToGroup(Groups.Field);

        this.sendButton.Connect(CommonSignals.Pressed, this, nameof(SendButtonClicked));
        this.resetButton.Connect(CommonSignals.Pressed, this, nameof(ResetButtonClicked));
        this.moveButtons.Visible = false;
        this.UpdateMoveButtons(0);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event.IsActionPressed("left_click"))
        {
            var selectedShip = this.GetTree().GetNodesInGroup(Groups.IsSelected)
                .Cast<Unit>()
                .FirstOrDefault();
            var selectedCell = this.field.WorldToMap(this.field.GetLocalMousePosition());
            if (this.field.GetCellv(selectedCell) == 5 && selectedShip?.TargetPositionMap != null)
            {
                this.GetTree().SetInputAsHandled();
                this.EmitSignal(nameof(MoveDone), selectedShip.TargetPositionMap.Value, selectedCell);
            }
            this.ClearSelection();
        }
    }

    private void SendButtonClicked()
    {
        this.EmitSignal(nameof(TurnConfirmed));
    }

    private void ResetButtonClicked()
    {
        this.EmitSignal(nameof(TurnCancelled));
    }

    private void UpdateMoveButtons(int pendingMoveCount)
    {
        var remainingMoves = this.maxMovesPerTurn == int.MaxValue
            ? "unlimited"
            : Math.Max(0, this.maxMovesPerTurn - pendingMoveCount).ToString();
        this.sendButton.Text = $"Send ({remainingMoves})";
        this.sendButton.Disabled = pendingMoveCount == 0;
    }

    public async Task Play(CancellationToken token = default)
    {
        await Game.Play(token).WrapCancellation(token);
    }
}
