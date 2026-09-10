using NUnit.Framework;
using TurnBase.KaNoBu;

[TestFixture]
public class KaNoBuBattleTests
{
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipStone, null, KaNoBuMoveNotificationModel.BattleResult.Draw)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipFlag, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipMine, null, KaNoBuMoveNotificationModel.BattleResult.BothDestroyed)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]

    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipPaper, null, KaNoBuMoveNotificationModel.BattleResult.Draw)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipFlag, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipMine, null, KaNoBuMoveNotificationModel.BattleResult.BothDestroyed)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]

    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipScissors, null, KaNoBuMoveNotificationModel.BattleResult.Draw)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipFlag, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipMine, null, KaNoBuMoveNotificationModel.BattleResult.BothDestroyed)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]

    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipFlag, KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipMine, null, KaNoBuMoveNotificationModel.BattleResult.BothDestroyed)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipUniversal, null, KaNoBuMoveNotificationModel.BattleResult.Draw)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipScout, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipFlag, KaNoBuFigure.FigureTypes.ShipScout, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipMine, null, KaNoBuMoveNotificationModel.BattleResult.BothDestroyed)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipScout, null, KaNoBuMoveNotificationModel.BattleResult.Draw)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout, KaNoBuFigure.FigureTypes.ShipUniversal, KaNoBuFigure.FigureTypes.ShipScout, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    public void BattlesAsExpected(KaNoBuFigure.FigureTypes attackerType, KaNoBuFigure.FigureTypes defenderType, KaNoBuFigure.FigureTypes? expectedWinnerType, KaNoBuMoveNotificationModel.BattleResult expectedBattleResult)
    {
        var attacker = KaNoBuFigure.Create(1, attackerType, true, 0);
        var defender = KaNoBuFigure.Create(2, defenderType, true, 0);

        var resolution = attacker.ResolveBattle(defender);

        Assert.That(resolution.Outcome, Is.EqualTo(expectedBattleResult));

        if (expectedWinnerType == null)
        {
            Assert.That(resolution.Winner, Is.Null);
            return;
        }

        Assert.That(resolution.Winner, Is.Not.Null);
        Assert.That(resolution.Winner.FigureType, Is.EqualTo(expectedWinnerType.Value));
        Assert.That(
            resolution.Winner.PlayerId,
            Is.EqualTo(expectedBattleResult == KaNoBuMoveNotificationModel.BattleResult.AttackerWon
                ? attacker.PlayerId
                : defender.PlayerId));
    }

    [TestCase(KaNoBuFigure.FigureTypes.ShipFlag)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipMine)]
    public void FlagAndMineCanNotInitializeBattle(KaNoBuFigure.FigureTypes attackerType)
    {
        foreach (KaNoBuFigure.FigureTypes defenderType in System.Enum.GetValues(typeof(KaNoBuFigure.FigureTypes)))
        {
            var attacker = KaNoBuFigure.Create(1, attackerType, true, 0);
            var defender = KaNoBuFigure.Create(2, defenderType, true, 0);

            var exception = Assert.Throws<System.Exception>(() => attacker.ResolveBattle(defender));
            var expectedMessage = defenderType == KaNoBuFigure.FigureTypes.Unknown
                ? "Can not resolve battle with unknown ship"
                : attackerType == KaNoBuFigure.FigureTypes.ShipFlag
                    ? "Flag can not initialize battle"
                    : "Mine can not initialize battle";

            Assert.That(exception.Message, Is.EqualTo(expectedMessage));
        }
    }
}
