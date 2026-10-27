using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class DefaultGameOverConditionTests
{
    [Test]
    public void DefaultConditionEndsWhenAtMostOnePlayerCanPlay()
    {
        var rules = new KaNoBuRules(6);

        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(), rules), Is.True);
        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(0), rules), Is.True);
        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(0, 1), rules), Is.False);
    }

    [Test]
    public void FindWinnersReturnsAllActivePlayers()
    {
        var rules = new KaNoBuRules(6);

        Assert.That(rules.FindWinners(CreateField(0, 1)), Is.EqualTo(new[] { 0, 1 }));
    }

    [Test]
    public void FindWinnersReturnsEmptyWhenEveryoneIsEliminated()
    {
        var rules = new KaNoBuRules(6);

        Assert.That(rules.FindWinners(CreateField()), Is.Empty);
    }

    [Test]
    public void FindWinnersExcludesPlayersWithOnlyImmobileFigures()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField(0);
        field.Field.SetFigure(0, 1, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipMine, 0));

        Assert.That(rules.IsGameOver(field), Is.True);
        Assert.That(rules.FindWinners(field), Is.Empty);
    }

    private static KaNoBuField CreateField(params int[] activePlayers)
    {
        var field = new KaNoBuField
        {
            Field = Field2D.Create(4, 2),
            DisconnectedPlayers = new HashSet<int>()
        };

        foreach (var playerNumber in activePlayers)
        {
            field.Field.SetFigure(playerNumber, 0, KaNoBuFigure.Create(playerNumber, KaNoBuFigure.FigureTypes.ShipFlag, 0));
            field.Field.SetFigure(playerNumber, 1, KaNoBuFigure.Create(playerNumber, KaNoBuFigure.FigureTypes.ShipStone, 0));
        }

        return field;
    }
}