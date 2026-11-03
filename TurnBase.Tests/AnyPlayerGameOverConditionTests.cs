using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class AnyPlayerGameOverConditionTests
{
    [Test]
    public void IsGameOverReturnsTrueWhenConditionMatchesAnyPlayer()
    {
        var condition = new AnyPlayerGameOverCondition(new PlayerCondition(3));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.True);
    }

    [Test]
    public void IsGameOverReturnsFalseWhenConditionMatchesNoPlayers()
    {
        var condition = new AnyPlayerGameOverCondition(new PlayerCondition(-1));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.False);
    }

    [Test]
    public void IsGameOverReturnsFalseWhenGameConditionNeverMatches()
    {
        var condition = new AnyPlayerGameOverCondition(new OrGameCondition());

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.False);
    }

    private static KaNoBuField CreateField()
    {
        return new KaNoBuField
        {
            Field = Field2D.Create(1, 1),
            DisconnectedPlayers = new HashSet<int>()
        };
    }

    private sealed class PlayerCondition : IGameCondition
    {
        private readonly int matchingPlayerId;

        public PlayerCondition(int matchingPlayerId)
        {
            this.matchingPlayerId = matchingPlayerId;
        }

        public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return playerNumber == this.matchingPlayerId;
        }
    }
}