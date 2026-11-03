using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class AndGameConditionTests
{
    [Test]
    public void CheckConditionReturnsTrueWhenAllConditionsAreTrue()
    {
        var condition = new AndGameCondition(new FixedCondition(true), new FixedCondition(true));

        Assert.That(condition.CheckCondition(null, null, 3), Is.True);
    }

    [Test]
    public void CheckConditionReturnsFalseWhenAnyConditionIsFalse()
    {
        var condition = new AndGameCondition(new FixedCondition(true), new FixedCondition(false));

        Assert.That(condition.CheckCondition(null, null, 3), Is.False);
    }

    [Test]
    public void CheckConditionReturnsTrueWhenThereAreNoConditions()
    {
        Assert.That(new AndGameCondition().CheckCondition(null, null, 3), Is.True);
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