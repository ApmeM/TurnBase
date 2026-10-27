using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class OrVictoryConditionTests
{
    [Test]
    public void IsWinnerReturnsTrueWhenAnyConditionIsTrue()
    {
        var condition = new OrVictoryCondition(new FixedCondition(false), new FixedCondition(true));

        Assert.That(condition.IsWinner(null, null, 3), Is.True);
    }

    [Test]
    public void IsWinnerReturnsFalseWhenAllConditionsAreFalse()
    {
        var condition = new OrVictoryCondition(new FixedCondition(false), new FixedCondition(false));

        Assert.That(condition.IsWinner(null, null, 3), Is.False);
    }

    [Test]
    public void EmptyOrConditionReturnsFalse()
    {
        Assert.That(new OrVictoryCondition().IsWinner(null, null, 3), Is.False);
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