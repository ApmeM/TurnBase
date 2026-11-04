using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class AndGameConditionTests
{
    [Test]
    public void CheckConditionReturnsTrueWhenAllConditionsAreTrue()
    {
        var condition = new AndGameCondition(new FixedGameCondition(true), new FixedGameCondition(true));

        Assert.That(condition.CheckCondition(null, null, 3), Is.True);
    }

    [Test]
    public void CheckConditionReturnsFalseWhenAnyConditionIsFalse()
    {
        var condition = new AndGameCondition(new FixedGameCondition(true), new FixedGameCondition(false));

        Assert.That(condition.CheckCondition(null, null, 3), Is.False);
    }

    [Test]
    public void CheckConditionReturnsTrueWhenThereAreNoConditions()
    {
        Assert.That(new AndGameCondition().CheckCondition(null, null, 3), Is.True);
    }

}