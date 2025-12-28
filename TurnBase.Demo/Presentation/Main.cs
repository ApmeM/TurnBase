using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBase.Core;
using TurnBase.KaNoBu;

public class Main : Node, IGameLogEventListener<KaNoBuInitResponseModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>
{
    [Export]
    public PackedScene UnitScene;

    public void GameLogFinished(List<int> winners, IField field)
    {
    }

    public void GameLogPlayerDisconnected(int playerNumber, IField field)
    {
    }

    public void GameLogPlayerInitialized(int playerNumber, InitResponseModel<KaNoBuInitResponseModel> initResponseModel, IField field)
    {
    }

    public void GameLogPlayerTurn(int playerNumber, KaNoBuMoveNotificationModel moveNotificationModel, KaNoBuMoveResponseModel moveResponseModel, IField field)
    {
        var allUnits = this.GetNode<Node2D>("Field").GetChildren();

        var fromMapPos = new Vector2(moveNotificationModel.move.From.X, moveNotificationModel.move.From.Y);
        var toMapPos = new Vector2(moveNotificationModel.move.To.X, moveNotificationModel.move.To.Y);
        var toWorldPos = this.GetNode<TileMap>("Level1").MapToWorld(toMapPos);

        var movedUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == fromMapPos);

        if (moveNotificationModel.battle.HasValue)
        {
            var attackerUnit = movedUnit;
            var defenderUnit = allUnits.Cast<Unit>().First(a => a.TargetPositionMap == toMapPos);

            if (moveNotificationModel.battle.Value.Item3 == null)
            {
                // Draw
            }
            else if (moveNotificationModel.battle.Value.Item1 == moveNotificationModel.battle.Value.Item3)
            {
                // Attacker won
                movedUnit.RotateUnitTo(toWorldPos);
                attackerUnit.Attack();
                defenderUnit.UnitHit();
                movedUnit.MoveUnitTo(toMapPos, toWorldPos);
            }
            else
            {
                // Defender won
                defenderUnit.RotateUnitTo(attackerUnit.Position);
                defenderUnit.Attack();
                attackerUnit.UnitHit();
            }
        }
        else
        {
            // No battle - swim here.
            movedUnit.RotateUnitTo(toWorldPos);
            movedUnit.MoveUnitTo(toMapPos, toWorldPos);

        }
    }

    public void GameLogPlayerWrongTurn(int playerNumber, MoveValidationStatus status, KaNoBuMoveResponseModel moveResponseModel, IField field)
    {
    }

    public void GameLogStarted(IField field)
    {
        List<Node2D> allUnits = new List<Node2D>();
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
                allUnits.Add(unitSceneInstance);
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

        var rules = new KaNoBuRules(8, 8);
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules);
        game.AddPlayer(new KaNoBuPlayerEasy());
        game.AddPlayer(new KaNoBuPlayerEasy());

        GameEventListenerConnector.Connect(game, this);

        await game.Play();

        this.GetNode<Button>("StartButton").Visible = true;
    }
}
