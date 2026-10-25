using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class GameOverConditionTests
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
    public void AndConditionRequiresEveryConditionToMatch()
    {
        var rules = new KaNoBuRules(6)
        {
            GameOverCondition = new AndGameOverCondition(
                new OnePlayerLeftGameOverCondition(),
                new OnePlayerLeftGameOverCondition()
            )
        };

        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(0), rules), Is.True);
        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(), rules), Is.False);
        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(0, 1), rules), Is.False);
    }

    [Test]
    public void OrConditionMatchesWhenAnyConditionMatches()
    {
        var rules = new KaNoBuRules(6)
        {
            GameOverCondition = new OrGameOverCondition(
                new AllPlayersEliminatedGameOverCondition(),
                new OnePlayerLeftGameOverCondition())
        };

        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(), rules), Is.True);
        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(0), rules), Is.True);
        Assert.That(rules.GameOverCondition.IsGameOver(CreateField(0, 1), rules), Is.False);
    }

    private static KaNoBuField CreateField(params int[] activePlayers)
    {
        var field = new KaNoBuField
        {
            Field = Field2D.Create(4, 2),
            DisconnectedPlayers = new List<int>()
        };

        foreach (var playerNumber in activePlayers)
        {
            field.Field.SetFigure(playerNumber, 0, KaNoBuFigure.Create(playerNumber, KaNoBuFigure.FigureTypes.ShipFlag, 0));
            field.Field.SetFigure(playerNumber, 1, KaNoBuFigure.Create(playerNumber, KaNoBuFigure.FigureTypes.ShipStone, 0));
        }

        return field;
    }
}