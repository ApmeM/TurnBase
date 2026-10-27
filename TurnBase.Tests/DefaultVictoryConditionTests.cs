using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class DefaultVictoryConditionTests
{
    [Test]
    public void IsWinnerReturnsTrueForPlayerWithMoveableFigure()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField(0);

        Assert.That(rules.VictoryCondition.IsWinner(field, rules, 0), Is.True);
    }

    [Test]
    public void IsWinnerReturnsFalseForPlayerWithoutMoveableFigure()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField(0);
        field.Field.SetFigure(0, 1, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipMine, 0));

        Assert.That(rules.VictoryCondition.IsWinner(field, rules, 0), Is.False);
    }

    [Test]
    public void IsWinnerReturnsFalseForDisconnectedPlayer()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField(0);
        field.DisconnectedPlayers.Add(0);

        Assert.That(rules.VictoryCondition.IsWinner(field, rules, 0), Is.False);
    }

    [Test]
    public void FindWinnersReturnsAllPlayersWithMoveableFigures()
    {
        var rules = new KaNoBuRules(6);

        Assert.That(rules.FindWinners(CreateField(0, 1)), Is.EqualTo(new[] { 0, 1 }));
    }

    [Test]
    public void FindWinnersReturnsEmptyWhenNoPlayerCanMove()
    {
        var rules = new KaNoBuRules(6);
        var field = CreateField(0);
        field.Field.SetFigure(0, 1, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipMine, 0));
        field.DisconnectedPlayers.Add(0);

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