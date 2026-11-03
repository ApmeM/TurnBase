using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class KaNoBuPlayerStatusTests
{
    [Test]
    public void NoMoveableFiguresConditionDetectsLoss()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField();
        field.Field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipFlag, 0));
        field.Field.SetFigure(1, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipMine, 0));
        field.Field.SetFigure(2, 0, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, 0));

        Assert.That(rules.LooseCondition.CheckCondition(field, rules, 0), Is.True);
        Assert.That(rules.LooseCondition.CheckCondition(field, rules, 1), Is.False);
    }

    [Test]
    public void AutoMoveSkipsPlayerWithoutMoveableFiguresButDoesNotRecordLoss()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField();
        field.Field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipFlag, 0));
        field.Field.SetFigure(1, 0, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, 0));

        var move = rules.AutoMove(field, 0);

        Assert.That(move, Is.Not.Null);
        Assert.That(move.Moves, Is.Empty);
        Assert.That(field.LostPlayers, Is.Empty, "Losses are evaluated after the turn-finished notification.");

        rules.IsGameOver(field);
        Assert.That(field.LostPlayers, Does.Contain(0));
    }

    [Test]
    public void PreviouslyLostPlayerIsAutoMovedEvenWithMoveableFigures()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField();
        field.Field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.LostPlayers.Add(0);

        var move = rules.AutoMove(field, 0);

        Assert.That(move, Is.Not.Null);
        Assert.That(move.Moves, Is.Empty);
    }

    [Test]
    public void AutoMoveDoesNotRecordLossForAPlayerWithoutMoveableFigures()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField();
        field.Field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));

        var move = rules.AutoMove(field, 3);

        Assert.That(move, Is.Not.Null);
        Assert.That(field.LostPlayers, Is.Empty);
    }

    [Test]
    public void BattleLossesAreRecordedWhenTheLastFiguresAreDestroyed()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField();
        field.Field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.Field.SetFigure(1, 0, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipMine, 0));
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point(0, 0), new Point(1, 0))
            });

        rules.TryApplyMoveResponse(field, 0, move);

        Assert.That(field.LostPlayers, Is.Empty, "Applying a move must not evaluate losses before the turn ends.");

        rules.IsGameOver(field);

        Assert.That(field.LostPlayers, Does.Contain(0));
        Assert.That(field.LostPlayers, Does.Contain(1));
    }

    [Test]
    public void AtMostOneActivePlayerGameOverConditionUsesThePlayerLossCondition()
    {
        var condition = new AtMostOnePlayerGameOverCondition(new NotGameCondition(new PlayerLooseGameCondtion()));
        var rules = new KaNoBuRules(6);
        var field = CreateField();
        field.LostPlayers.UnionWith(new[] { 0, 1, 2 });

        Assert.That(condition.IsGameOver(field, rules), Is.True);

        field.LostPlayers.Remove(2);
        Assert.That(condition.IsGameOver(field, rules), Is.False);
    }

    [Test]
    public void FindWinnersIncludesDisconnectedPlayerWhenCustomConditionMatches()
    {
        var rules = new KaNoBuRules(6)
        {
            VictoryCondition = new TerritoryVictoryCondition(0)
        };
        var field = CreateField();
        field.DisconnectedPlayers.Add(0);
        field.LostPlayers.Add(0);

        Assert.That(rules.FindWinners(field), Does.Contain(0));
    }

    private static KaNoBuField CreateField()
    {
        return new KaNoBuField
        {
            Field = Field2D.Create(4, 2),
            DisconnectedPlayers = new HashSet<int>(),
            LostPlayers = new HashSet<int>()
        };
    }

    private sealed class TerritoryVictoryCondition : IGameCondition
    {
        private readonly int territoryOwner;

        public TerritoryVictoryCondition(int territoryOwner)
        {
            this.territoryOwner = territoryOwner;
        }

        public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return playerNumber == this.territoryOwner;
        }
    }
}