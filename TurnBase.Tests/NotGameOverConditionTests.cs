using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class NotGameOverConditionTests
{
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void IsGameOverReturnsInvertedResult(bool innerResult, bool expectedResult)
    {
        var condition = new NotGameOverCondition(new FixedCondition(innerResult));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.EqualTo(expectedResult));
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