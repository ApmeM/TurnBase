using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;


[SceneReference("GameField.tscn")]
public partial class GameField :
    IGameLogEventListener<KaNoBuInitResponseModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>,
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
{
    [Export]
    public PackedScene UnitScene;
    private int playerId = -1;
    private KaNoBuFieldMemorization memorizedField = new KaNoBuFieldMemorization();

    #region IPlayer region

    public Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
    {
        GD.Print("Init start");
        this.playerId = model.PlayerId;
        _ = MoveCameraToPlayer();
        return new KaNoBuPlayerEasy().Init(model);
    }

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
    {
        GD.Print("MakeTurn start");
        this.timerLabel.ShowMessage("Your turn", 1f);

        this.memorizedField.SynchronizeField(model.Request.Field);

        this.UpdateKnownShips();

        var level = this.water;

        GD.Print("Waiting for drag finished");
        var dragRes = await this.drag.ToSignal(this.drag, nameof(DragControl.DragFinished));
        GD.Print($"Drag finished from {(Vector2)dragRes[0]} to {(Vector2)dragRes[1]}");
        var from = level.WorldToMap(level.ToLocal((Vector2)dragRes[0]));
        var to = level.WorldToMap(level.ToLocal((Vector2)dragRes[1]));
        GD.Print($"Move {from} to {to}");

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(
            new KaNoBuMoveResponseModel(
                KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN,
                new Point { X = (int)from.x, Y = (int)from.y },
                new Point { X = (int)to.x, Y = (int)to.y }
            ));
    }

    #endregion

    public async Task MoveCameraToPlayer()
    {
        var tween = new Tween();
        this.AddChild(tween);
        Vector2 cameraCenter;
        switch (playerId)
        {
            case 0:
                cameraCenter = new Vector2(this.GetViewport().Size.x * 2 / 4, this.GetViewport().Size.y * 1 / 4);
                break;
            case 1:
                cameraCenter = new Vector2(this.GetViewport().Size.x * 2 / 4, this.GetViewport().Size.y * 3 / 4);
                break;
            case 2:
                cameraCenter = new Vector2(this.GetViewport().Size.x * 1 / 4, this.GetViewport().Size.y * 2 / 4);
                break;
            case 3:
                cameraCenter = new Vector2(this.GetViewport().Size.x * 3 / 4, this.GetViewport().Size.y * 2 / 4);
                break;
            default:
                cameraCenter = new Vector2(this.GetViewport().Size.x * 2 / 4, this.GetViewport().Size.y * 2 / 4);
                break;
        }

        tween.InterpolateProperty(this.draggableCamera, "position", this.draggableCamera.Position, cameraCenter, 1f);
        tween.InterpolateProperty(this.draggableCamera, "zoom", this.draggableCamera.Scale, Vector2.One / 1.3f, 1f);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
        tween.QueueFree();
    }
    public async Task MoveCameraToCenter()
    {
        var tween = new Tween();
        this.AddChild(tween);

        var cameraCenter = new Vector2(this.GetViewport().Size.x / 2, this.GetViewport().Size.y / 2);

        tween.InterpolateProperty(this.draggableCamera, "position", this.draggableCamera.Position, cameraCenter, 1f);
        tween.InterpolateProperty(this.draggableCamera, "scale", this.draggableCamera.Scale, Vector2.One, 1f);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
        tween.QueueFree();
    }

    #region IGameEventListener region

    public async void GameStarted()
    {
        this.playerId = -1;

        this.field.RemoveChildren();
        this.memorizedField.Clear();

        this.timerLabel.ShowMessage("Game Started.", 1f);
    }

    public void GamePlayerInit(int playerNumber, string playerName)
    {
        GD.Print($"Log: Player {playerNumber} ({playerName}) joined the game.");

        this.field.RemoveChildren();
        this.memorizedField.Clear();
    }

    public void GameLogPlayerInit(int playerNumber, KaNoBuInitResponseModel initResponseModel)
    {
        GD.Print($"Log: Player {playerNumber} initialized.");

        this.field.RemoveChildren();
        this.memorizedField.Clear();
    }

    public void PlayersInitialized(IField mainField)
    {
        this.memorizedField.Clear();
        this.memorizedField.SynchronizeField(mainField);

        this.field.RemoveChildren();
        var level = this.water;

        for (var x = 0; x < mainField.Width; x++)
        {
            for (var y = 0; y < mainField.Height; y++)
            {
                var originalShip = mainField.get(x, y) as KaNoBuFigure;
                if (originalShip == null)
                {
                    continue;
                }

                var mapPos = new Vector2(x, y);
                var worldPos = level.MapToWorld(mapPos);
                var unit = (Unit)UnitScene.Instance();

                unit.TargetPositionMap = mapPos;
                unit.Position = worldPos + level.CellSize / 2;
                unit.PlayerNumber = originalShip.PlayerId;
                unit.UnitType = originalShip.FigureType;
                unit.Connect(nameof(Unit.UnitClicked), this, nameof(OnUnitClicked), new Godot.Collections.Array { unit });

                this.field.AddChild(unit);
            }
        }
    }

    public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification)
    {
        this.memorizedField.UpdateKnownShips(notification);

        if (notification.move.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
        {
            GD.Print($"Move {playerNumber} Skip turn.");
            return;
        }
        GD.Print($"Move {playerNumber} from {notification.move.From.PrintableName()} to {notification.move.To.PrintableName()}");

        var fromMapPos = new Vector2(notification.move.From.X, notification.move.From.Y);
        var toMapPos = new Vector2(notification.move.To.X, notification.move.To.Y);
        var level = this.water;
        var toWorldPos = level.MapToWorld(toMapPos) + level.CellSize / 2;

        var allUnits = this.field.GetChildren();
        var movedUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == fromMapPos && a.PlayerNumber == playerNumber);

        if (notification.battle.HasValue)
        {
            GD.Print($"Battle result: {notification.battle.Value.battleResult} (IsFlag = {notification.battle.Value.isDefenderFlag})");
            var defenderUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == toMapPos);

            GD.Print($"moved unit {movedUnit.PlayerNumber} {movedUnit.UnitType}");
            GD.Print($"defender unit {defenderUnit.PlayerNumber} {defenderUnit.UnitType}");

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
                    defenderUnit.RotateUnitTo(movedUnit.Position);
                    defenderUnit.Attack();
                    movedUnit.UnitHit();
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

    public void GameLogPlayerTurn(int playerNumber, KaNoBuMoveResponseModel moveResponseModel, MoveValidationStatus status)
    {
        if (status != MoveValidationStatus.OK)
        {
            GD.Print($"Wrong turn made: {status}");
        }
        else
        {
            GD.Print("Correct turn made.");
        }
    }

    public void GameTurnFinished()
    {
        GD.Print($"Turn finished.");
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

    public void GameLogCurrentField(IField field)
    {
        this.memorizedField.SynchronizeField(field);
        GD.Print(field.ToString());
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

            var figure = this.memorizedField.Field.get((int)unit.TargetPositionMap.Value.x, (int)unit.TargetPositionMap.Value.y) as KaNoBuFigure;
            unit.PlayerNumber = figure.PlayerId;
            unit.UnitType = figure.FigureType;
        }
    }

    private void OnUnitClicked(Unit unit)
    {
        if (unit.PlayerNumber != this.playerId)
        {
            return;
        }

        var drag = this.drag;
        drag.StartDragging();
    }

    public override void _Ready()
    {
        this.FillMembers();
    }
}
