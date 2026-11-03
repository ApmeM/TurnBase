using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class AtMostOnePlayerGameOverConditionTests
{
    [Test]
    public void IsGameOverIgnoresDisconnectedPlayersWhenCountingMatches()
    {
        var condition = new AtMostOnePlayerGameOverCondition(
            new AndGameCondition(
                new PlayerHasMoveableFiguresGameCondition(),
                new NotGameCondition(new PlayerDisconnectedGameCondition())));

        var field = CreateField();
        field.Field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.Field.SetFigure(1, 0, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.DisconnectedPlayers.Add(1);

        Assert.That(condition.IsGameOver(field, new KaNoBuRules(6)), Is.True);
    }

    [Test]
    public void IsGameOverReturnsFalseWhenMoreThanOnePlayerMatchesCondition()
    {
        var condition = new AtMostOnePlayerGameOverCondition(new PlayerHasMoveableFiguresGameCondition());
        var field = CreateField();
        field.Field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.Field.SetFigure(1, 0, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, 0));

        Assert.That(condition.IsGameOver(field, new KaNoBuRules(6)), Is.False);
    }

    [Test]
    public void IsGameOverReturnsTrueWhenNoPlayersMatchCondition()
    {
        var condition = new AtMostOnePlayerGameOverCondition(new PlayerHasMoveableFiguresGameCondition());

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.True);
    }

    private static KaNoBuField CreateField()
    {
        return new KaNoBuField
        {
            Field = Field2D.Create(4, 1),
            DisconnectedPlayers = new HashSet<int>()
        };
    }
}