using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;


[SceneReference("Main.tscn")]
public partial class Main :
    IGameLogEventListener<KaNoBuInitResponseModel, KaNoBuMoveResponseModel>,
    IGameEventListener<KaNoBuMoveNotificationModel>,
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>
{
    [Export]
    public PackedScene UnitScene;


    #region IPlayer region

    public Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
    {
        GD.Print("Init start");
        return new KaNoBuPlayerEasy().Init(model);
    }

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
    {
        GD.Print("MakeTurn start");
        this.yourTurnLabel.Visible = true;
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

        this.yourTurnLabel.Visible = false;
        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(
            new KaNoBuMoveResponseModel(
                KaNoBuMoveResponseModel.MoveStatus.MAKE_TURN,
                new Point { X = (int)from.x, Y = (int)from.y },
                new Point { X = (int)to.x, Y = (int)to.y }
            ));
    }

    #endregion

    #region IGameEventListener region

    public void GameStarted()
    {
        GD.Print("Game Started.");
    }

    public void GamePlayerInit(int playerNumber, string playerName)
    {
        GD.Print($"Player {playerNumber} initialized.");

        var field = this.field;
        var allUnits = field.GetChildren();
        foreach (Node2D unit in allUnits)
        {
            field.RemoveChild(unit);
            unit.QueueFree();
        }
    }

    public void GameLogPlayerInit(int playerNumber, KaNoBuInitResponseModel initResponseModel)
    {
        GD.Print($"Log: Player {playerNumber} initialized.");
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


        if (notification.battle != null)
        {
            GD.Print($"Battle result: {notification.battle.Value.battleResult} (IsFlag = {notification.battle.Value.isDefenderFlag})");
        }

        var fromMapPos = new Vector2(notification.move.From.X, notification.move.From.Y);
        var toMapPos = new Vector2(notification.move.To.X, notification.move.To.Y);
        var level = this.water;
        var toWorldPos = level.MapToWorld(toMapPos) + level.CellSize / 2;

        GD.Print($"Looking for unit of player {playerNumber} at {fromMapPos}");
        var movedUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == fromMapPos && a.PlayerNumber == playerNumber);

        if (notification.battle.HasValue)
        {
            GD.Print($"Looking for defender unit at {toMapPos}");
            var defenderUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == toMapPos);

            switch (notification.battle.Value.battleResult)
            {
                case KaNoBuMoveNotificationModel.BattleResult.Draw:
                    if (movedUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.UnitType = movedUnit.UnitType;
                    if (defenderUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.UnitType = defenderUnit.UnitType;
                    break;
                case KaNoBuMoveNotificationModel.BattleResult.AttackerWon:
                    GD.Print($"Set new position for unit at {movedUnit.TargetPositionMap} to {toMapPos}");
                    GD.Print($"Set new position for unit at {defenderUnit.TargetPositionMap} to {null}");
                    // Attacker won

                    if (notification.battle.Value.isDefenderFlag)
                    {
                        defenderUnit.UnitType = KaNoBuFigure.FigureTypes.ShipFlag;
                        // Attacker is unknown - any ship can beat the flag.
                        // All the units under this player control changes the owner.
                        allUnits.Cast<Unit>().Where(a => a.PlayerNumber == defenderUnit.PlayerNumber).ToList().ForEach(a => a.PlayerNumber = movedUnit.PlayerNumber);
                    }
                    else
                    {
                        if (movedUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.UnitType = Looser[movedUnit.UnitType];
                        if (defenderUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.UnitType = Winner[defenderUnit.UnitType];
                    }

                    movedUnit.RotateUnitTo(toWorldPos);
                    movedUnit.Attack();
                    defenderUnit.UnitHit();
                    movedUnit.MoveUnitTo(toMapPos, toWorldPos);
                    break;
                case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                    GD.Print($"Set new position for unit at {movedUnit.TargetPositionMap} to {null}");
                    // Defender won
                    if (movedUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) defenderUnit.UnitType = Winner[movedUnit.UnitType];
                    if (defenderUnit.UnitType != KaNoBuFigure.FigureTypes.Unknown) movedUnit.UnitType = Looser[defenderUnit.UnitType];

                    defenderUnit.RotateUnitTo(movedUnit.Position);
                    defenderUnit.Attack();
                    movedUnit.UnitHit();
                    break;
            }
        }
        else
        {
            GD.Print($"Set new position for unit at {movedUnit.TargetPositionMap} to {toMapPos}");
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

    public void GameFinished(List<int> winners)
    {
        this.uI.Visible = true;

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

    private Dictionary<KaNoBuFigure.FigureTypes, KaNoBuFigure.FigureTypes> Winner = new Dictionary<KaNoBuFigure.FigureTypes, KaNoBuFigure.FigureTypes>
    {
        {KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipScissors},
        {KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipStone},
        {KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipPaper},
    };
    private Dictionary<KaNoBuFigure.FigureTypes, KaNoBuFigure.FigureTypes> Looser = new Dictionary<KaNoBuFigure.FigureTypes, KaNoBuFigure.FigureTypes>
    {
        {KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipStone},
        {KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipPaper},
        {KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipScissors},
    };

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
                unit.Rotation = Mathf.Pi;
                unit.Position = worldPos + level.CellSize / 2;
                unit.PlayerNumber = originalShip.Value.PlayerNumber;
                unit.UnitType = originalShip.Value.FigureType;

                this.field.AddChild(unit);
            }
        }
    }

    private void UpdateKnownShips(KaNoBuMoveModel.FigureModel?[,] field)
    {
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
        var drag = this.drag;
        drag.StartDragging();
    }

    public override void _Ready()
    {
        this.FillMembers();        

        this.gameType.Connect("item_selected", this, nameof(GameTypeChanged));
        this.startButton.Connect("pressed", this, nameof(StartButonClicked));


        this.serverIpInfo.Text = string.Join("\n", IP.GetLocalAddresses().Cast<string>().Where(a => !a.Contains(":")));
    }

    private void GameTypeChanged(int selectedId)
    {
        switch (this.gameType.GetSelectedId())
        {
            case 0:
                // server
                this.serverGameType.Visible = true;
                this.clientGameType.Visible = false;
                break;
            case 1:
                // client
                this.serverGameType.Visible = false;
                this.clientGameType.Visible = true;
                break;
            default:
                throw new Exception("Unknown game type");
        }
    }

    private async void StartButonClicked()
    {
        var allUnits = this.field.GetChildren();
        foreach (Unit unit in allUnits)
        {
            unit.QueueFree();
        }

        this.uI.Visible = false;

        switch (this.gameType.GetSelectedId())
        {
            case 0:
                // server
                var rules = new KaNoBuRules(8);
                var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules, "test");

                var playerTypes = new[]{
                    this.serverPlayer1,
                    this.serverPlayer2,
                    this.serverPlayer3,
                    this.serverPlayer4,
                };

                var humanFound = false;
                foreach (var playertype in playerTypes)
                {
                    switch (playertype.GetSelectedId())
                    {
                        case 0:
                            // None
                            game.AddPlayer(new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>());
                            continue;
                        case 1:
                            // Human
                            if (humanFound)
                            {
                                GD.Print("2 Human players are not implemented yet.");
                                return;
                            }
                            humanFound = true;
                            game.AddPlayer(this);
                            continue;
                        case 2:
                            // Computer Easy
                            game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 300, this));
                            continue;
                        case 3:
                            // Remote
                            this.server.StartServer();
                            var player = new ServerPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(server, game.GameId);
                            game.AddPlayer(player);
                            game.AddGameListener(player);
                            continue;
                        default:
                            throw new InvalidOperationException("Unknown Player Type");
                    }
                }

                game.AddGameListener(this);
                if (!humanFound)
                {
                    game.AddGameLogListener(this);
                }

                await game.Play();

                break;
            case 1:
                this.client.ServerUrl = $"http://{this.serverIpInput.Text}:8080";
                await this.client.StartPolling<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(this, "test");
                return;
            default:
                throw new InvalidOperationException("Unknown game type");
        }

        this.uI.Visible = true;
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

        if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipPaper)
        {
            return figure.PlayerId + "P";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipScissors)
        {
            return figure.PlayerId + "S";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipStone)
        {
            return figure.PlayerId + "R";
        }
        else if (ship.FigureType == KaNoBuFigure.FigureTypes.ShipFlag)
        {
            return figure.PlayerId + "F";
        }

        throw new Exception("Unknown ship type: " + ship.FigureType);
    }
}
