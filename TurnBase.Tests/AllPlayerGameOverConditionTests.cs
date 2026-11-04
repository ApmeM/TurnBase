using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class AllPlayerGameOverConditionTests
{
    [Test]
    public void IsGameOverReturnsTrueWhenEveryPlayerMeetsTheCondition()
    {
        var condition = new AllPlayerGameOverCondition(new FixedGameCondition(true));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.True);
    }

    [Test]
    public void IsGameOverReturnsFalseWhenConditionIsFalseForEveryPlayer()
    {
        var condition = new AllPlayerGameOverCondition(new FixedGameCondition(false));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.False);
    }

    private static KaNoBuField CreateField()
    {
        return new KaNoBuField { Field = Field2D.Create(1, 1), DisconnectedPlayers = new HashSet<int>() };
    }

}