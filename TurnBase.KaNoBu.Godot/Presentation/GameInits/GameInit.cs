using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("GameInit.tscn")]
public partial class GameInit : IGameInit
{
    [Export]
    public PackedScene UnitScene;

    private TaskCompletionSource<Field2D> completion;

    private InitModel<KaNoBuInitModel> model;

    private Random r = new Random();


    public async Task<InitResponseModel<KaNoBuInitResponseModel>> Run(InitModel<KaNoBuInitModel> model, CancellationToken token = default)
    {
        if (this.model != null)
        {
            throw new InvalidOperationException("GameInit is already running");
        }

        this.model = model;
        this.completion = new TaskCompletionSource<Field2D>();

        this.field.RemoveChildren();

        this.timerLabel.ShowMessage("Place your ships", 1f);

        for (var x = 0; x < model.Request.Width; x++)
        {
            for (var y = 0; y < model.Request.Height; y++)
            {
                this.field.SetCellv(new Vector2(x, y), 4);
            }
        }

        this.field.Position = new Vector2(5 - model.Request.Width / 2, 1) * this.field.CellSize;

        for (var f = 0; f < model.Request.AvailableFigures.Count; f++)
        {
            var x = f % model.Request.Width;
            var y = f / model.Request.Width;

            var originalShip = model.Request.AvailableFigures[f];

            var mapPos = new Vector2(x, y);
            var worldPos = this.field.MapToWorld(mapPos + new Vector2(0, 8));
            var unit = (Unit)UnitScene.Instance();

            unit.PlayerNumber = model.PlayerId;
            unit.UnitType = originalShip;
            unit.Position = worldPos + this.field.CellSize / 2;
            unit.IsClickable = true;
            unit.Connect(nameof(Unit.UnitClicked), this, nameof(OnUnitClicked), new Godot.Collections.Array { unit });

            this.field.AddChild(unit);
        }

        using (token.Register(() => this.completion.TrySetCanceled()))
        {
            try
            {
                var field = await this.completion.Task;
                return new InitResponseModel<KaNoBuInitResponseModel>
                {
                    Response = new KaNoBuInitResponseModel(field)
                };
            }
            finally
            {
                this.model = null;
                this.completion = null;
                this.ClearSelection();
                this.field.RemoveChildren();
            }
        }
    }

    private void ShowSelection(Unit unit)
    {
        for (var x = 0; x < this.model.Request.Width; x++)
        {
            for (var y = 0; y < this.model.Request.Height; y++)
            {
                this.field.SetCellv(new Vector2(x, y), 5);
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

        if (from != to && this.field.GetCellv(to) == 5)
        {
            this.ClearSelection();
            this.MoveShip(unit, to);
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
        this.randomButton.Connect(CommonSignals.Pressed, this, nameof(RandomButtonClicked));

        this.beach.GetUsedCells()
            .Cast<Vector2>()
            .Where(point => this.castle.GetCellv(point) == -1)
            .ToList()
            .ForEach(point => this.castle.SetCellv(point, 6));
        this.castle.UpdateBitmaskRegion();
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
                this.MoveShip(selectedShip, selectedCell);
            }
            this.ClearSelection();
        }
    }

    private void MoveShip(Unit unitFrom, Vector2 toMap)
    {
        var fromMap = unitFrom.TargetPositionMap;
        var fromPos = unitFrom.Position;
        var toPos = this.field.MapToWorld(toMap) + this.field.CellSize / 2;

        var centerBottomMap = new Vector2(this.model.Request.Width / 2 - 1, 7);
        var centerBottomPos = this.field.MapToWorld(centerBottomMap) + this.field.CellSize / 2;
        var centerTopMap = new Vector2(this.model.Request.Width / 2 - 1, 5);
        var centerTopPos = this.field.MapToWorld(centerTopMap) + this.field.CellSize / 2;

        var unitTo = this.field.GetChildren()
            .OfType<Unit>()
            .SingleOrDefault(a => a.TargetPositionMap == toMap && a.TargetPositionMap.HasValue);

        unitFrom.MoveUnitToLogic(toMap);
        unitFrom.CancelAnimations();
        if (unitFrom.Position.y >= centerBottomPos.y)
        {
            unitFrom.CallbackAnimation((u) => u.RotateUnitToAnimation(centerBottomPos));
            unitFrom.CallbackAnimation((u) => u.MoveUnitToAnimation(centerBottomPos));
        }
        if (unitFrom.Position.y >= centerTopPos.y)
        {
            unitFrom.CallbackAnimation((u) => u.RotateUnitToAnimation(centerTopPos));
            unitFrom.CallbackAnimation((u) => u.MoveUnitToAnimation(centerTopPos));
        }
        unitFrom.CallbackAnimation((u) => u.RotateUnitToAnimation(toPos));
        unitFrom.CallbackAnimation((u) => u.MoveUnitToAnimation(toPos));

        unitTo?.MoveUnitToLogic(fromMap);
        unitTo?.CancelAnimations();
        if (unitTo?.Position.y < centerTopPos.y && !fromMap.HasValue)
        {
            unitTo?.CallbackAnimation((u) => u.RotateUnitToAnimation(centerTopPos));
            unitTo?.CallbackAnimation((u) => u.MoveUnitToAnimation(centerTopPos));
        }
        if (unitTo?.Position.y < centerBottomPos.y && !fromMap.HasValue)
        {
            unitTo?.CallbackAnimation((u) => u.RotateUnitToAnimation(centerBottomPos));
            unitTo?.CallbackAnimation((u) => u.MoveUnitToAnimation(centerBottomPos));
        }
        unitTo?.CallbackAnimation((u) => u.RotateUnitToAnimation(fromPos));
        unitTo?.CallbackAnimation((u) => u.MoveUnitToAnimation(fromPos));

        UpdateBattleButton();
    }

    private void SendButtonClicked()
    {
        var units = this.field.GetChildren().OfType<Unit>();
        if (units.Any(a => !a.TargetPositionMap.HasValue) || this.completion == null)
        {
            return;
        }

        var result = Field2D.Create(this.model.Request.Width, this.model.Request.Height);
        foreach (var unit in units)
        {
            var position = unit.TargetPositionMap.Value;
            result[(int)position.x, (int)position.y] = KaNoBuFigure.Create(this.model.PlayerId, unit.UnitType, 0);
        }

        this.completion.TrySetResult(result);
    }

    private void RandomButtonClicked()
    {
        var positions = new List<Vector2>();
        for (var x = 0; x < this.model.Request.Width; x++)
        {
            for (var y = 0; y < this.model.Request.Height; y++)
            {
                positions.Add(new Vector2(x, y));
            }
        }

        var centerBottomMap = new Vector2(this.model.Request.Width / 2 - 1, 7);
        var centerBottomPos = this.field.MapToWorld(centerBottomMap) + this.field.CellSize / 2;
        var centerTopMap = new Vector2(this.model.Request.Width / 2 - 1, 5);
        var centerTopPos = this.field.MapToWorld(centerTopMap) + this.field.CellSize / 2;

        var units = this.field.GetChildren().OfType<Unit>().ToList();
        for (var i = positions.Count - 1; i >= 0; i--)
        {
            var index = this.r.Next(i + 1);

            var toPos = this.field.MapToWorld(positions[index]) + this.field.CellSize / 2;
            var toMap = positions[index];
            units[i].MoveUnitToLogic(toMap);

            units[i].MoveUnitToLogic(toMap);
            units[i].CancelAnimations();
            if (units[i].Position.y > centerBottomPos.y)
            {
                units[i].CallbackAnimation((u) => u.RotateUnitToAnimation(centerBottomPos));
                units[i].CallbackAnimation((u) => u.MoveUnitToAnimation(centerBottomPos));
            }
            if (units[i].Position.y > centerTopPos.y)
            {
                units[i].CallbackAnimation((u) => u.RotateUnitToAnimation(centerTopPos));
                units[i].CallbackAnimation((u) => u.MoveUnitToAnimation(centerTopPos));
            }
            units[i].CallbackAnimation((u) => u.RotateUnitToAnimation(toPos));
            units[i].CallbackAnimation((u) => u.MoveUnitToAnimation(toPos));

            positions.RemoveAt(index);
        }

        this.UpdateBattleButton();
    }

    private void UpdateBattleButton()
    {
        var units = this.field.GetChildren().OfType<Unit>();
        this.sendButton.Disabled = units.Any(a => !a.TargetPositionMap.HasValue);
    }
}
