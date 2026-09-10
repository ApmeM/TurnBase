using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class KaNoBuFieldMemorizationTests
{
    private static readonly Point AttackerPosition = new Point { X = 0, Y = 0 };
    private static readonly Point DefenderPosition = new Point { X = 1, Y = 0 };

    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.Unknown, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.Unknown, KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.AttackerWon)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.Unknown, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    [TestCase(KaNoBuFigure.FigureTypes.Unknown, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuMoveNotificationModel.BattleResult.DefenderWon)]
    public void BattleResultRevealsTheMissingRegularShip(
        KaNoBuFigure.FigureTypes attackerType,
        KaNoBuFigure.FigureTypes defenderType,
        KaNoBuFigure.FigureTypes expectedWinnerType,
        KaNoBuMoveNotificationModel.BattleResult battleResult)
    {
        var field = UpdateMemory(attackerType, defenderType, battleResult);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(expectedWinnerType));
    }

    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, KaNoBuFigure.FigureTypes.ShipPaper)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, KaNoBuFigure.FigureTypes.ShipScissors)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, KaNoBuFigure.FigureTypes.ShipStone)]
    public void KnownRegularAttackerRevealsUniqueDefender(
        KaNoBuFigure.FigureTypes attackerType,
        KaNoBuFigure.FigureTypes expectedDefenderType)
    {
        var field = UpdateMemory(
            attackerType,
            KaNoBuFigure.FigureTypes.Unknown,
            KaNoBuMoveNotificationModel.BattleResult.DefenderWon);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(expectedDefenderType));
    }

    [Test]
    public void ScoutAttackerKeepsUnknownDefenderWhenItLoses()
    {
        var field = UpdateMemory(
            KaNoBuFigure.FigureTypes.ShipScout,
            KaNoBuFigure.FigureTypes.Unknown,
            KaNoBuMoveNotificationModel.BattleResult.DefenderWon);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(KaNoBuFigure.FigureTypes.Unknown));
    }

    [Test]
    public void UnknownAttackerKeepsUnknownTypeWhenItDefeatsScout()
    {
        var field = UpdateMemory(
            KaNoBuFigure.FigureTypes.Unknown,
            KaNoBuFigure.FigureTypes.ShipScout,
            KaNoBuMoveNotificationModel.BattleResult.AttackerWon);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(KaNoBuFigure.FigureTypes.Unknown));
    }

    [Test]
    public void ScoutDrawRevealsTheOtherScout()
    {
        var field = UpdateMemory(
            KaNoBuFigure.FigureTypes.ShipScout,
            KaNoBuFigure.FigureTypes.Unknown,
            KaNoBuMoveNotificationModel.BattleResult.Draw);

        Assert.That(GetFigureType(field, AttackerPosition), Is.EqualTo(KaNoBuFigure.FigureTypes.ShipScout));
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(KaNoBuFigure.FigureTypes.ShipScout));
    }

    [Test]
    public void AttackingWithUniversalShipRevealsItsEffectiveType()
    {
        var field = UpdateMemory(
            KaNoBuFigure.FigureTypes.ShipUniversal,
            KaNoBuFigure.FigureTypes.ShipStone,
            KaNoBuMoveNotificationModel.BattleResult.AttackerWon);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(KaNoBuFigure.FigureTypes.ShipPaper));
    }

    [Test]
    public void DefendingWithUniversalShipRevealsItsEffectiveType()
    {
        var field = UpdateMemory(
            KaNoBuFigure.FigureTypes.ShipStone,
            KaNoBuFigure.FigureTypes.ShipUniversal,
            KaNoBuMoveNotificationModel.BattleResult.DefenderWon);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(KaNoBuFigure.FigureTypes.ShipPaper));
    }

    [Test]
    public void CapturingFlagMovesAttackerToDestination()
    {
        var field = UpdateMemory(
            KaNoBuFigure.FigureTypes.ShipPaper,
            KaNoBuFigure.FigureTypes.Unknown,
            KaNoBuMoveNotificationModel.BattleResult.AttackerWon,
            isDefenderFlag: true);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(KaNoBuFigure.FigureTypes.ShipPaper));
    }

    [Test]
    public void BothDestroyedRemovesBothShips()
    {
        var field = UpdateMemory(
            KaNoBuFigure.FigureTypes.ShipPaper,
            KaNoBuFigure.FigureTypes.ShipMine,
            KaNoBuMoveNotificationModel.BattleResult.BothDestroyed);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(field[DefenderPosition], Is.Null);
    }

    [Test]
    public void BattleShipTypesAreAppliedBeforeUpdatingMemory()
    {
        var field = UpdateMemory(
            KaNoBuFigure.FigureTypes.Unknown,
            KaNoBuFigure.FigureTypes.Unknown,
            KaNoBuMoveNotificationModel.BattleResult.AttackerWon,
            attackerBattleType: KaNoBuFigure.FigureTypes.ShipPaper,
            defenderBattleType: KaNoBuFigure.FigureTypes.ShipStone);

        Assert.That(field[AttackerPosition], Is.Null);
        Assert.That(GetFigureType(field, DefenderPosition), Is.EqualTo(KaNoBuFigure.FigureTypes.ShipPaper));
    }

    private static Field2D UpdateMemory(
        KaNoBuFigure.FigureTypes attackerType,
        KaNoBuFigure.FigureTypes defenderType,
        KaNoBuMoveNotificationModel.BattleResult battleResult,
        bool isDefenderFlag = false,
        KaNoBuFigure.FigureTypes attackerBattleType = KaNoBuFigure.FigureTypes.Unknown,
        KaNoBuFigure.FigureTypes defenderBattleType = KaNoBuFigure.FigureTypes.Unknown)
    {
        var field = Field2D.Create(2, 1);
        field[AttackerPosition] = KaNoBuFigure.Create(1, attackerType, false, 0);
        field[DefenderPosition] = KaNoBuFigure.Create(2, defenderType, false, 0);

        var memorization = new KaNoBuFieldMemorization();
        memorization.SynchronizeField(field);

        var battle = new KaNoBuMoveNotificationModel.Battle
        {
            battleResult = battleResult,
            isDefenderFlag = isDefenderFlag,
            attackerFigureType = attackerBattleType,
            defenderFigureType = defenderBattleType,
        };
        var notification = new KaNoBuMoveNotificationModel(
            new List<KaNoBuMoveNotificationModel.MoveNotification>
            {
                new KaNoBuMoveNotificationModel.MoveNotification(AttackerPosition, DefenderPosition, battle),
            });

        memorization.UpdateKnownShips(notification);
        return memorization.Field;
    }

    private static KaNoBuFigure.FigureTypes GetFigureType(Field2D field, Point position)
    {
        return ((KaNoBuFigure)field[position]).FigureType;
    }
}