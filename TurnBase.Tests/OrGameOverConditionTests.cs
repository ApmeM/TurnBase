using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class OrGameOverConditionTests
{
    [Test]
    public void IsGameOverReturnsTrueWhenAnyConditionIsTrue()
    {
        var condition = new OrGameOverCondition(new FixedCondition(false), new FixedCondition(true));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.True);
    }

    [Test]
    public void IsGameOverReturnsFalseWhenAllConditionsAreFalse()
    {
        var condition = new OrGameOverCondition(new FixedCondition(false), new FixedCondition(false));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.False);
    }

    [Test]
    public void EmptyOrConditionReturnsFalse()
    {
        var condition = new OrGameOverCondition();

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.False);
    }

    private static KaNoBuField CreateField()
    {
        return new KaNoBuField { Field = Field2D.Create(1, 1), DisconnectedPlayers = new HashSet<int>() };
    }

    private sealed class FixedCondition : IGameOverCondition
    {
        private readonly bool result;

        public FixedCondition(bool result)
        {
            this.result = result;
        }

        public bool IsGameOver(KaNoBuField mainField, KaNoBuRules rules)
        {
            return this.result;
        }
    }
}