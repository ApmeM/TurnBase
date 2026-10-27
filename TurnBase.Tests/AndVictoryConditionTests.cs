using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class AndVictoryConditionTests
{
    [Test]
    public void IsWinnerReturnsTrueWhenAllConditionsAreTrue()
    {
        var condition = new AndVictoryCondition(new FixedCondition(true), new FixedCondition(true));

        Assert.That(condition.IsWinner(null, null, 3), Is.True);
    }

    [Test]
    public void IsWinnerReturnsFalseWhenAnyConditionIsFalse()
    {
        var condition = new AndVictoryCondition(new FixedCondition(true), new FixedCondition(false));

        Assert.That(condition.IsWinner(null, null, 3), Is.False);
    }

    [Test]
    public void EmptyAndConditionReturnsTrue()
    {
        Assert.That(new AndVictoryCondition().IsWinner(null, null, 3), Is.True);
    }

    private sealed class FixedCondition : IVictoryCondition
    {
        private readonly bool result;

        public FixedCondition(bool result)
        {
            this.result = result;
        }

        public bool IsWinner(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return this.result;
        }
    }
}