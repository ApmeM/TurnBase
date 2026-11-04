using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class NotGameConditionTests
{
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void CheckConditionReturnsInvertedResult(bool innerResult, bool expectedResult)
    {
        var condition = new NotGameCondition(new FixedGameCondition(innerResult));

        Assert.That(condition.CheckCondition(null, null, 3), Is.EqualTo(expectedResult));
    }

}