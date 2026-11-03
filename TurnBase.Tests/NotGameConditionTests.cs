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
        var condition = new NotGameCondition(new FixedCondition(innerResult));

        Assert.That(condition.CheckCondition(null, null, 3), Is.EqualTo(expectedResult));
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