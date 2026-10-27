using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class NotVictoryConditionTests
{
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void IsWinnerReturnsInvertedResult(bool innerResult, bool expectedResult)
    {
        var condition = new NotVictoryCondition(new FixedCondition(innerResult));

        Assert.That(condition.IsWinner(null, null, 3), Is.EqualTo(expectedResult));
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