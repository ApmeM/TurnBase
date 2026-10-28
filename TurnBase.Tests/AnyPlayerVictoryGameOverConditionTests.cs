using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class AnyPlayerVictoryGameOverConditionTests
{
    [Test]
    public void IsGameOverReturnsTrueWhenVictoryConditionSucceedsForAnyPlayer()
    {
        var condition = new AnyPlayerVictoryGameOverCondition(new PlayerVictoryCondition(3));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.True);
    }

    [Test]
    public void IsGameOverReturnsFalseWhenVictoryConditionFailsForEveryPlayer()
    {
        var condition = new AnyPlayerVictoryGameOverCondition(new PlayerVictoryCondition(4));

        Assert.That(condition.IsGameOver(CreateField(), new KaNoBuRules(6)), Is.False);
    }

    [Test]
    public void IsGameOverReturnsFalseWhenComposedConditionHasNoAlternatives()
    {
        var condition = new AnyPlayerVictoryGameOverCondition(new OrVictoryCondition());

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

    private sealed class PlayerVictoryCondition : IVictoryCondition
    {
        private readonly int winningPlayerId;

        public PlayerVictoryCondition(int winningPlayerId)
        {
            this.winningPlayerId = winningPlayerId;
        }

        public bool IsWinner(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            return playerNumber == this.winningPlayerId;
        }
    }
}