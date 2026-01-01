using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

public class Main : Node, IGameLogEventListener<KaNoBuInitResponseModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
{
    [Export]
    public PackedScene UnitScene;

    public void GameLogFinished(List<int> winners, IField field)
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

    public void GameLogPlayerInitialized(int playerNumber, InitResponseModel<KaNoBuInitResponseModel> initResponseModel, IField field)
    {
    }

    public void GameLogPlayerTurn(int playerNumber, KaNoBuMoveNotificationModel moveNotificationModel, KaNoBuMoveResponseModel moveResponseModel, IField field)
    {
        if (moveNotificationModel.move.Status == KaNoBuMoveResponseModel.MoveStatus.SKIP_TURN)
        {
            GD.Print($"Move {playerNumber} Skip turn.");
            GD.Print(showField(field));
            return;
        }
        GD.Print($"Move {playerNumber} from {showPoint(moveNotificationModel.move.From)} to {showPoint(moveNotificationModel.move.To)}");

        var allUnits = this.GetNode<Node2D>("Field").GetChildren();

        if (moveNotificationModel.battle != null)
        {
            GD.Print($"Battle: attacker: {getShipResource(moveNotificationModel.battle.Value.Item1)}, defender: {getShipResource(moveNotificationModel.battle.Value.Item2)}, winner: {getShipResource(moveNotificationModel.battle.Value.Item3)}");
        }

        var fromMapPos = new Vector2(moveNotificationModel.move.From.X, moveNotificationModel.move.From.Y);
        var toMapPos = new Vector2(moveNotificationModel.move.To.X, moveNotificationModel.move.To.Y);
        var toWorldPos = this.GetNode<TileMap>("Level1").MapToWorld(toMapPos);

        GD.Print($"Looking for unit of player {playerNumber} at {fromMapPos}");
        var movedUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == fromMapPos && a.PlayerNumber == playerNumber);

        if (moveNotificationModel.battle.HasValue)
        {
            GD.Print($"Looking for unit of player {moveNotificationModel.battle.Value.Item2.PlayerId} at {toMapPos}");
            var defenderUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == toMapPos && a.PlayerNumber == moveNotificationModel.battle.Value.Item2.PlayerId);

            if (moveNotificationModel.battle.Value.Item3 == null)
            {
                // Draw
            }
            else if (moveNotificationModel.battle.Value.Item1 == moveNotificationModel.battle.Value.Item3)
            {
                GD.Print($"Set new position for unit at {movedUnit.TargetPositionMap} to {toMapPos}");
                GD.Print($"Set new position for unit at {defenderUnit.TargetPositionMap} to {null}");
                // Attacker won
                movedUnit.RotateUnitTo(toWorldPos);
                movedUnit.Attack();
                defenderUnit.UnitHit();
                movedUnit.MoveUnitTo(toMapPos, toWorldPos);
            }
            else
            {
                GD.Print($"Set new position for unit at {movedUnit.TargetPositionMap} to {null}");
                // Defender won
                defenderUnit.RotateUnitTo(movedUnit.Position);
                defenderUnit.Attack();
                movedUnit.UnitHit();
            }
        }
        else
        {
            GD.Print($"Set new position for unit at {movedUnit.TargetPositionMap} to {toMapPos}");
            // No battle - swim here.
            movedUnit.RotateUnitTo(toWorldPos);
            movedUnit.MoveUnitTo(toMapPos, toWorldPos);
        }
        GD.Print(showField(field));
    }

    public void GameLogPlayerWrongTurn(int playerNumber, MoveValidationStatus status, KaNoBuMoveResponseModel moveResponseModel, IField field)
    {
    }

    public void GameLogStarted(IField field)
    {
        var allUnits = this.GetNode<Node2D>("Field").GetChildren();
        foreach (Unit unit in allUnits)
        {
            unit.QueueFree();
        }

        for (var x = 0; x < field.Width; x++)
        {
            for (var y = 0; y < field.Height; y++)
            {
                var ship = (KaNoBuFigure)field.get(new Point { X = x, Y = y });
                if (ship == null)
                {
                    continue;
                }

                var mapPos = new Vector2(x, y);
                var worldPos = this.GetNode<TileMap>("Level1").MapToWorld(mapPos);

                var unitSceneInstance = (Unit)UnitScene.Instance();
                unitSceneInstance.UnitType = ship.FigureType;
                unitSceneInstance.PlayerNumber = ship.PlayerId;
                unitSceneInstance.Rotation = Mathf.Pi;
                unitSceneInstance.Position = worldPos;

                this.GetNode<Node2D>("Field").AddChild(unitSceneInstance);
                unitSceneInstance.MoveUnitTo(mapPos, worldPos);
            }
        }
    }

    public void GameLogTurnFinished(IField field)
    {
    }

    public async override void _Ready()
    {
        this.GetNode<Button>("StartButton").Connect("pressed", this, nameof(StartButonClicked));
    }

    private async void StartButonClicked()
    {
        this.GetNode<Button>("StartButton").Visible = false;

        var rules = new KaNoBuRules(8);
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules);
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 300, this));
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 300, this));
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 300, this));
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 300, this));

        GameEventListenerConnector.Connect(game, this);

        await game.Play();

        this.GetNode<Button>("StartButton").Visible = true;
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
