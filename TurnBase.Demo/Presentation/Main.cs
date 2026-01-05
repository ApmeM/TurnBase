using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

public class Main : Node,
    IGameLogEventListener<KaNoBuInitResponseModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>,
    IGameEventListener<KaNoBuMoveNotificationModel>,
    IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>
{
    [Export]
    public PackedScene UnitScene;

    public void GameLogFinished(List<int> winners, IField field)
    {
        this.GameFinished(winners);
    }

    public void GameFinished(List<int> winners)
    {
        if (winners.Count == 1)
        {
            GD.Print($"Player {winners[0]} won.");
        }
        else if (winners.Count == 0)
        {
            GD.Print($"Draw.");
        }
        else
        {
            throw new Exception("Unexpected number of winners.");
        }
    }

    public void GameLogPlayerDisconnected(int playerNumber, IField field)
    {
    }

    public void GamePlayerDisconnected(int playerNumber)
    {
    }

    public void GameLogPlayerInitialized(int playerNumber, InitResponseModel<KaNoBuInitResponseModel> initResponseModel, IField field)
    {
    }

    public void GamePlayerInitialized(int playerNumber, string playerName)
    {
    }

    public void GameLogPlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification, KaNoBuMoveResponseModel moveResponseModel, IField field)
    {
        this.GamePlayerTurn(playerNumber, notification);
        GD.Print(showField(field));
    }

    public void GamePlayerTurn(int playerNumber, KaNoBuMoveNotificationModel notification)
    {
        if (notification.move.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
        {
            GD.Print($"Move {playerNumber} Skip turn.");
            return;
        }
        GD.Print($"Move {playerNumber} from {showPoint(notification.move.From)} to {showPoint(notification.move.To)}");

        var allUnits = this.GetNode<Node2D>("Field").GetChildren();

        if (notification.battle != null)
        {
            GD.Print($"Battle result: {notification.battle.Value.Item1} (IsFlag = {notification.battle.Value.Item2})");
        }

        var fromMapPos = new Vector2(notification.move.From.X, notification.move.From.Y);
        var toMapPos = new Vector2(notification.move.To.X, notification.move.To.Y);
        var level = this.GetNode<TileMap>("Water");
        var toWorldPos = level.MapToWorld(toMapPos) + level.CellSize / 2;

        GD.Print($"Looking for unit of player {playerNumber} at {fromMapPos}");
        var movedUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == fromMapPos && a.PlayerNumber == playerNumber);

        if (notification.battle.HasValue)
        {
            GD.Print($"Looking for defender unit at {toMapPos}");
            var defenderUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == toMapPos);

            switch (notification.battle.Value.Item1)
            {
                case KaNoBuMoveNotificationModel.BattleResult.Draw:
                    if (movedUnit.UnitType != null) defenderUnit.UnitType = movedUnit.UnitType.Value;
                    if (defenderUnit.UnitType != null) movedUnit.UnitType = defenderUnit.UnitType.Value;
                    break;
                case KaNoBuMoveNotificationModel.BattleResult.AttackerWon:
                    GD.Print($"Set new position for unit at {movedUnit.TargetPositionMap} to {toMapPos}");
                    GD.Print($"Set new position for unit at {defenderUnit.TargetPositionMap} to {null}");
                    // Attacker won
                    movedUnit.RotateUnitTo(toWorldPos);
                    movedUnit.Attack();
                    defenderUnit.UnitHit();
                    movedUnit.MoveUnitTo(toMapPos, toWorldPos);

                    if (notification.battle.Value.Item2)
                    {
                        defenderUnit.UnitType = KaNoBuFigure.FigureTypes.ShipFlag;
                        // Attacker is unknown - any ship can beat the flag.
                        // All the units under this player control changes the owner.
                        allUnits.Cast<Unit>().Where(a => a.PlayerNumber == defenderUnit.PlayerNumber).ToList().ForEach(a => a.PlayerNumber = movedUnit.PlayerNumber);
                    }
                    else
                    {
                        if (movedUnit.UnitType != null) defenderUnit.UnitType = Looser[movedUnit.UnitType.Value];
                        if (defenderUnit.UnitType != null) movedUnit.UnitType = Winner[defenderUnit.UnitType.Value];
                    }
                    break;
                case KaNoBuMoveNotificationModel.BattleResult.DefenderWon:
                    GD.Print($"Set new position for unit at {movedUnit.TargetPositionMap} to {null}");
                    // Defender won
                    defenderUnit.RotateUnitTo(movedUnit.Position);
                    defenderUnit.Attack();
                    movedUnit.UnitHit();

                    if (movedUnit.UnitType != null) defenderUnit.UnitType = Winner[movedUnit.UnitType.Value];
                    if (defenderUnit.UnitType != null) movedUnit.UnitType = Looser[defenderUnit.UnitType.Value];

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

    public void GameLogPlayerWrongTurn(int playerNumber, MoveValidationStatus status, KaNoBuMoveResponseModel moveResponseModel, IField field)
    {
        this.GamePlayerWrongTurn(playerNumber, status);
    }

    public void GamePlayerWrongTurn(int playerNumber, MoveValidationStatus status)
    {
        GD.Print($"Wrong turn made: {status}");
    }

    public void GameLogStarted(IField field)
    {
        var level = this.GetNode<TileMap>("Water");
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
                unit.UnitType = (originalShip as KaNoBuFigure)?.FigureType;

                this.GetNode<Node2D>("Field").AddChild(unit);
            }
        }
    }

    public void GameStarted()
    {
    }

    public void GameLogTurnFinished(IField field)
    {
    }

    public void GameTurnFinished()
    {
    }

    public Task<InitResponseModel<KaNoBuInitResponseModel>> Init(InitModel<KaNoBuInitModel> model)
    {
        return new KaNoBuPlayerEasy().Init(model);
    }

    public async Task<MakeTurnResponseModel<KaNoBuMoveResponseModel>> MakeTurn(MakeTurnModel<KaNoBuMoveModel> model)
    {
        GD.Print("MakeTurn start");
        var allUnits = this.GetNode<Node2D>("Field").GetChildren();

        if (allUnits.Count == 0)
        {
            this.GameLogStarted(model.Request.Field);

            allUnits = this.GetNode<Node2D>("Field").GetChildren();
            foreach (Unit unit in allUnits)
            {
                unit.Connect(nameof(Unit.UnitClicked), this, nameof(OnUnitClicked), new Godot.Collections.Array { unit });
            }
        }
        else
        {
            this.UpdateKnownShips(model.Request.Field);
        }

        var drag = this.GetNode<DragControl>("Drag");
        var level = this.GetNode<TileMap>("Water");

        GD.Print("Waiting for drag finished");
        var dragRes = await drag.ToSignal(drag, nameof(DragControl.DragFinished));
        GD.Print($"Drag finished from {(Vector2)dragRes[0]} to {(Vector2)dragRes[1]}");
        var from = level.WorldToMap(level.ToLocal((Vector2)dragRes[0]));
        var to = level.WorldToMap(level.ToLocal((Vector2)dragRes[1]));

        return new MakeTurnResponseModel<KaNoBuMoveResponseModel>(
            new KaNoBuMoveResponseModel(
                new Point { X = (int)from.x, Y = (int)from.y },
                new Point { X = (int)to.x, Y = (int)to.y }
            ));
    }

    private void UpdateKnownShips(IField field)
    {
        var allUnits = this.GetNode<Node2D>("Field").GetChildren();
        foreach (Unit unit in allUnits)
        {
            if (unit.TargetPositionMap == null)
            {
                continue;
            }

            var figure = field.get(new Point { X = (int)unit.TargetPositionMap.Value.x, Y = (int)unit.TargetPositionMap.Value.y });
            unit.PlayerNumber = figure.PlayerId;
            unit.UnitType = unit.UnitType ?? (figure as KaNoBuFigure)?.FigureType;
        }
    }

    private void OnUnitClicked(Unit unit)
    {
        var drag = this.GetNode<DragControl>("Drag");
        drag.StartDragging();
    }

    public override void _Ready()
    {
        this.GetNode<OptionButton>("UI/GameType").Connect("item_selected", this, nameof(GameTypeChanged));
        this.GetNode<Button>("UI/StartButton").Connect("pressed", this, nameof(StartButonClicked));
    }

    private void GameTypeChanged(int selectedId)
    {
        switch (this.GetNode<OptionButton>("UI/GameType").GetSelectedId())
        {
            case 0:
                // server
                this.GetNode<OptionButton>("UI/ServerPlayer1").Visible = true;
                this.GetNode<OptionButton>("UI/ServerPlayer2").Visible = true;
                this.GetNode<OptionButton>("UI/ServerPlayer3").Visible = true;
                this.GetNode<OptionButton>("UI/ServerPlayer4").Visible = true;
                this.GetNode<OptionButton>("UI/ClientPlayer").Visible = false;
                break;
            case 1:
                // client
                this.GetNode<OptionButton>("UI/ServerPlayer1").Visible = false;
                this.GetNode<OptionButton>("UI/ServerPlayer2").Visible = false;
                this.GetNode<OptionButton>("UI/ServerPlayer3").Visible = false;
                this.GetNode<OptionButton>("UI/ServerPlayer4").Visible = false;
                this.GetNode<OptionButton>("UI/ClientPlayer").Visible = true;
                break;
            default:
                throw new Exception("Unknown game type");
        }
    }

    private async void StartButonClicked()
    {
        var rules = new KaNoBuRules(8);
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules);

        var humanFound = false;

        switch (this.GetNode<OptionButton>("UI/GameType").GetSelectedId())
        {
            case 0:
                // server
                var playerTypes = Enumerable.Range(1, 4)
                    .Select(a => $"UI/ServerPlayer{a}")
                    .Select(a => this.GetNode<OptionButton>(a))
                    .Select(a => a.GetSelectedId());

                foreach (var playertype in playerTypes)
                {
                    switch (playertype)
                    {
                        case 0:
                            game.AddPlayer(new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>());
                            continue;
                        case 1:
                            if (humanFound)
                            {
                                GD.Print("2 Human players are not implemented yet.");
                                return;
                            }
                            humanFound = true;
                            game.AddPlayer(this);
                            continue;
                        case 2:
                            game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 300, this));
                            continue;
                        case 3:
                            GD.Print("Player type 'Remote' is not implemented yet.");
                            return;
                        default:
                            throw new InvalidOperationException("Unknown Player Type");
                    }
                }
                break;
            case 1:
                GD.Print("Game type 'client' is not implemented yet.");
                return;
            default:
                throw new InvalidOperationException("Unknown game type");
        }

        this.GetNode<Control>("UI").Visible = false;

        if (humanFound)
        {
            GameEventListenerConnector.ConnectPlayer(game, this);
        }
        else
        {
            GameEventListenerConnector.ConnectListener(game, this);
        }

        var allUnits = this.GetNode<Node2D>("Field").GetChildren();
        foreach (Unit unit in allUnits)
        {
            unit.QueueFree();
        }

        await game.Play();

        this.GetNode<Control>("UI").Visible = true;
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
