using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class OrGameConditionTests
{
    [Test]
    public void CheckConditionReturnsTrueWhenAnyConditionIsTrue()
    {
        var condition = new OrGameCondition(new FixedGameCondition(false), new FixedGameCondition(true));

        Assert.That(condition.CheckCondition(null, null, 3), Is.True);
    }

    [Test]
    public void CheckConditionReturnsFalseWhenAllConditionsAreFalse()
    {
        var condition = new OrGameCondition(new FixedGameCondition(false), new FixedGameCondition(false));

        Assert.That(condition.CheckCondition(null, null, 3), Is.False);
    }

    [Test]
    public void CheckConditionReturnsFalseWhenThereAreNoConditions()
    {
        Assert.That(new OrGameCondition().CheckCondition(null, null, 3), Is.False);
    }

}