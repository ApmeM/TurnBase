using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class OrGameConditionTests
{
    [Test]
    public void CheckConditionReturnsTrueWhenAnyConditionIsTrue()
    {
        var condition = new OrGameCondition(new FixedCondition(false), new FixedCondition(true));

        Assert.That(condition.CheckCondition(null, null, 3), Is.True);
    }

    [Test]
    public void CheckConditionReturnsFalseWhenAllConditionsAreFalse()
    {
        var condition = new OrGameCondition(new FixedCondition(false), new FixedCondition(false));

        Assert.That(condition.CheckCondition(null, null, 3), Is.False);
    }

    [Test]
    public void CheckConditionReturnsFalseWhenThereAreNoConditions()
    {
        Assert.That(new OrGameCondition().CheckCondition(null, null, 3), Is.False);
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