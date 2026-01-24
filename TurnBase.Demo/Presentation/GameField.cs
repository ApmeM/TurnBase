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
        var allUnits = this.field.GetChildren();

        if (allUnits.Count == 0)
        {
            this.InitializeField(model.Request.Field);

            allUnits = this.field.GetChildren();
            foreach (Unit unit in allUnits)
            {
                unit.Connect(nameof(Unit.UnitClicked), this, nameof(OnUnitClicked), new Godot.Collections.Array { unit });
            }
        }
        else
        {
            this.UpdateKnownShips(model.Request.Field);
        }

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

        this.timerLabel.ShowMessage("Game Started.", 1f);
    }

    public void GamePlayerInit(int playerNumber, string playerName)
    {
        GD.Print($"Log: Player {playerNumber} ({playerName}) joined the game.");

        this.field.RemoveChildren();
    }

    public void GameLogPlayerInit(int playerNumber, KaNoBuInitResponseModel initResponseModel)
    {
        GD.Print($"Log: Player {playerNumber} initialized.");
    
        this.field.RemoveChildren();
    }

    public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification)
    {
        var allUnits = this.field.GetChildren();
        if (allUnits.Count == 0)
        {
            // Field is not yet initialized.
            return;
        }

        if (notification.move.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
        {
            GD.Print($"Move {playerNumber} Skip turn.");
            return;
        }
        GD.Print($"Move {playerNumber} from {showPoint(notification.move.From)} to {showPoint(notification.move.To)}");

        var fromMapPos = new Vector2(notification.move.From.X, notification.move.From.Y);
        var toMapPos = new Vector2(notification.move.To.X, notification.move.To.Y);
        var level = this.water;
        var toWorldPos = level.MapToWorld(toMapPos) + level.CellSize / 2;

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
                    if (movedUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.UnitType = movedUnit.UnitType;
                    if (defenderUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.UnitType = defenderUnit.UnitType;
                    break;
                case KaNoBuMoveNotificationModel.BattleResult.AttackerWon:
                    // Attacker won

                    if (movedUnit.UnitType == KaNoBuFigure.FigureTypes.ShipUniversal)
                    {
                        movedUnit.UnitType = KaNoBuFigure.FigureTypes.Unknown;
                    }
                    if (notification.battle.Value.isDefenderFlag)
                    {
                        defenderUnit.UnitType = KaNoBuFigure.FigureTypes.ShipFlag;
                        // Attacker is unknown - any ship can beat the flag.
                        // All the units under this player control changes the owner.
                        allUnits.Cast<Unit>().Where(a => a.PlayerNumber == defenderUnit.PlayerNumber).ToList().ForEach(a => a.PlayerNumber = movedUnit.PlayerNumber);
                    }
                    else
                    {
                        if (movedUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.UnitType = KaNoBuRules.Looser[movedUnit.UnitType];
                        if (defenderUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.UnitType = KaNoBuRules.Winner[defenderUnit.UnitType];
                    }

                    movedUnit.RotateUnitTo(toWorldPos);
                    movedUnit.Attack();
                    defenderUnit.UnitHit();
                    movedUnit.MoveUnitTo(toMapPos, toWorldPos);
                    break;
                case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                    // Defender won
                    if (defenderUnit.UnitType == KaNoBuFigure.FigureTypes.ShipUniversal)
                    {
                        defenderUnit.UnitType = KaNoBuFigure.FigureTypes.Unknown;
                    }

                    if (movedUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.UnitType = KaNoBuRules.Winner[movedUnit.UnitType];
                    if (defenderUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.UnitType = KaNoBuRules.Looser[defenderUnit.UnitType];

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

    [Signal]
    public delegate void GameFinishedEventHandler();

    public async void GameFinished(List<int> winners)
    {
        this.EmitSignal(nameof(GameFinishedEventHandler));

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
        GD.Print(showField(field));
        var allUnits = this.field.GetChildren();

        if (allUnits.Count == 0)
        {
            var level = this.water;
            for (var x = 0; x < field.Width; x++)
            {
                for (var y = 0; y < field.Height; y++)
                {
                    var originalShip = field.get(new Point { X = x, Y = y });
                    if (originalShip == null)
                    {
                        continue;
                    }

                    var mapPos = new Vector2(x, y);
                    var worldPos = level.MapToWorld(mapPos);
                    var unit = (Unit)UnitScene.Instance();

                    unit.TargetPositionMap = mapPos;
                    unit.Rotation = Mathf.Pi;
                    unit.Position = worldPos + level.CellSize / 2;
                    unit.PlayerNumber = originalShip.PlayerId;
                    unit.UnitType = (originalShip as KaNoBuFigure)?.FigureType ?? KaNoBuFigure.FigureTypes.Unknown;

                    this.field.AddChild(unit);
                }
            }
        }
    }

    #endregion


    private void InitializeField(KaNoBuMoveModel.FigureModel?[,] field)
    {
        var level = this.water;
        for (var x = 0; x < field.GetLength(0); x++)
        {
            for (var y = 0; y < field.GetLength(1); y++)
            {
                var originalShip = field[x, y];
                if (originalShip == null)
                {
                    continue;
                }

                var mapPos = new Vector2(x, y);
                var worldPos = level.MapToWorld(mapPos);
                var unit = (Unit)UnitScene.Instance();

                unit.TargetPositionMap = mapPos;
                unit.Position = worldPos + level.CellSize / 2;
                unit.PlayerNumber = originalShip.Value.PlayerNumber;
                unit.UnitType = originalShip.Value.FigureType;

                this.field.AddChild(unit);
            }
        }
    }

    private void UpdateKnownShips(KaNoBuMoveModel.FigureModel?[,] field)
    {
        // In case player defeated and all its ships become known.
        var allUnits = this.field.GetChildren();
        foreach (Unit unit in allUnits)
        {
            if (unit.TargetPositionMap == null)
            {
                continue;
            }

            var figure = field[(int)unit.TargetPositionMap.Value.x, (int)unit.TargetPositionMap.Value.y];
            unit.PlayerNumber = figure.Value.PlayerNumber;
            if (unit.UnitType == KaNoBuFigure.FigureTypes.Unknown)
            {
                unit.UnitType = figure.Value.FigureType;
            }
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

    private string showPoint(Point point)
    {
        return $"({(char)('A' + point.X)}{point.Y})";
    }

    private string showField(IField field)
    {
        string result = "";
        result += string.Format("   ");
        for (int j = 0; j < field.Width; j++)
        {
            result += $"  {(char)('A' + j)}";
        }
        result += string.Format("   ");
        result += "\n";

        for (int i = 0; i < field.Height; i++)
        {
            result += $"  {i}";
            for (int j = 0; j < field.Width; j++)
            {
                var ship = field.get(new Point { X = j, Y = i });
                result += $" {getShipResource(ship)}";
            }

            result += $"   {i}\n";
        }

        result += string.Format("   ");
        for (int j = 0; j < field.Width; j++)
        {
            result += $"  {(char)('A' + j)}";
        }
        result += string.Format("   ");

        return result;
    }

    private string getShipResource(IFigure figure)
    {
        if (figure == null)
        {
            return "  ";
        }
        else if (figure is UnknownFigure)
        {
            return figure.PlayerId + "?";
        }

        var ship = (KaNoBuFigure)figure;
        return figure.PlayerId + ship.FigureType.PrintableName();
    }
}
