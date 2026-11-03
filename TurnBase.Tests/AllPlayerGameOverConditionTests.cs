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
        var condition = new AllPlayerGameOverCondition(
            new AndGameCondition(new FixedCondition(true), new FixedCondition(true)));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.True);
    }

    [Test]
    public void IsGameOverReturnsFalseWhenCombinedConditionIsFalseForEveryPlayer()
    {
        var condition = new AllPlayerGameOverCondition(
            new AndGameCondition(new FixedCondition(true), new FixedCondition(false)));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.False);
    }

    private static KaNoBuField CreateField()
    {
        return new KaNoBuField { Field = Field2D.Create(1, 1), DisconnectedPlayers = new HashSet<int>() };
    }

    private sealed class FixedCondition : IGameCondition
    {
        private readonly bool result;

        public FixedCondition(bool result)
        {
            this.result = result;
        }

        public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return this.result;
        }
    }
}