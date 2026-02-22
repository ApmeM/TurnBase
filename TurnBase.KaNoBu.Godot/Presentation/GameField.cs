using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("GameField.tscn")]
public partial class GameField :
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
{
    [Export]
    public PackedScene UnitScene;
    private int playerId = -1;
    private KaNoBuFieldMemorization memorizedField = new KaNoBuFieldMemorization();
    public IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> Game;

    public TileMap Water => this.water;

    #region IPlayer region

    public Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
    {
        this.playerId = model.PlayerId;
        _ = MoveCameraToPlayer();
        return new KaNoBuPlayerEasy().Init(model);
    }

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
    {
        this.timerLabel.ShowMessage("Your turn", 1f);

        this.memorizedField.SynchronizeField((Field2D)model.Request.Field);

        this.UpdateKnownShips();

        var moveRes = await this.ToSignal(this, nameof(MoveDone));
        var from = (Vector2)moveRes[0];
        var to = (Vector2)moveRes[1];

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>
        {
            Response = new KaNoBuMoveResponseModel(
                KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN,
                new Point { X = (int)from.x, Y = (int)from.y },
                new Point { X = (int)to.x, Y = (int)to.y }
            )
        };
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

    public virtual void GameLogCurrentField(IField field)
    {
        var mainField = (Field2D)field;
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

                    unit.TargetPositionMap = mapPos;
                    unit.Position = worldPos + this.field.CellSize / 2;
                    unit.Connect(nameof(Unit.UnitClicked), this, nameof(OnUnitClicked), new Godot.Collections.Array { unit });

                    this.field.AddChild(unit);
                }
            }
        }

        this.UpdateKnownShips();
    }

    public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification)
    {
        this.memorizedField.UpdateKnownShips(notification);

        if (notification.move.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
        {
            return;
        }

        var fromMapPos = new Vector2(notification.move.From.X, notification.move.From.Y);
        var toMapPos = new Vector2(notification.move.To.X, notification.move.To.Y);
        var toWorldPos = this.field.MapToWorld(toMapPos) + this.field.CellSize / 2;

        var allUnits = this.field.GetChildren();
        var movedUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == fromMapPos && a.PlayerNumber == playerNumber);

        if (notification.battle.HasValue)
        {
            var defenderUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == toMapPos);

            switch (notification.battle.Value.battleResult)
            {
                case KaNoBuMoveNotificationModel.BattleResult.Draw:
                    break;
                case KaNoBuMoveNotificationModel.BattleResult.AttackerWon:
                    // Attacker won
                    movedUnit.RotateUnitTo(toWorldPos);
                    movedUnit.Attack();
                    defenderUnit.UnitHit();
                    movedUnit.MoveUnitTo(toMapPos, toWorldPos);
                    break;
                case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                    // Defender won
                    if (notification.battle.Value.isMine)
                    {
                        movedUnit.RotateUnitTo(toWorldPos);
                        movedUnit.Attack();
                        movedUnit.UnitHit();
                        defenderUnit.UnitHit();
                    }
                    else
                    {
                        defenderUnit.RotateUnitTo(movedUnit.Position);
                        defenderUnit.Attack();
                        movedUnit.UnitHit();
                    }
                    break;
            }
        }
        else
        {
            // No battle - swim here.
            movedUnit.RotateUnitTo(toWorldPos);
            movedUnit.MoveUnitTo(toMapPos, toWorldPos);
        }

        this.UpdateKnownShips();
    }

    public void GameTurnFinished()
    {
    }

    public async void GameFinished(List<int> winners)
    {
        if (winners.Count == 1)
        {
            this.timerLabel.ShowMessage($"Player {winners[0]} won.", 5);
        }
        else if (winners.Count == 0)
        {
            this.timerLabel.ShowMessage($"Draw.", 5);
        }
        else
        {
            throw new Exception("Unexpected number of winners.");
        }
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

    public Vector2 WorldToMap(Vector2 position)
    {
        return this.field.WorldToMap(position);
    }

    public async Task Play()
    {
        await Game.Play();
    }
}
