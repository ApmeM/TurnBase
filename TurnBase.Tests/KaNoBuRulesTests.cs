using System.Collections.Generic;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class KaNoBuRulesTests
{
    [Test]
    public void PromotionKeepsCorrectBattleResult()
    {
        var field = Field2D.Create(2, 2);
        var attacker = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, true, 2);
        var defender = KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipScissors, true, 0);
        field[0, 0] = attacker;
        field[1, 0] = defender;

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            });

        var notification = rules.MakeMove(field, 1, move);

        Assert.That(notification.MoveNotifications.Count, Is.EqualTo(1));
        Assert.That(notification.MoveNotifications[0].Battle, Is.Not.Null);
        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult, Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.AttackerWon));

        var placed = (KaNoBuFigure)field[1, 0];
        Assert.That(placed, Is.Not.Null);
        Assert.That(placed.FigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipUniversal));
        Assert.That(placed.PlayerId, Is.EqualTo(1));
    }

    [Test]
    public void BattleResultUsesPlayerIdentityWhenWinnerInstanceChanges()
    {
        var field = Field2D.Create(2, 2);
        var attacker = new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipUniversal, true, 0);
        var defender = KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipStone, true, 0);
        field[0, 0] = attacker;
        field[1, 0] = defender;

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            });

        var notification = rules.MakeMove(field, 1, move);

        Assert.That(notification.MoveNotifications.Count, Is.EqualTo(1));
        Assert.That(notification.MoveNotifications[0].Battle, Is.Not.Null);
        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult, Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.AttackerWon));
    }

    [Test]
    public void BattleWithMineDestroysBothFigures()
    {
        var field = Field2D.Create(2, 2);
        field[0, 0] = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, true, 0);
        field[1, 0] = KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipMine, true, 0);

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            });

        var notification = rules.MakeMove(field, 1, move);

        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult, Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.BothDestroyed));
        Assert.That(field[0, 0], Is.Null);
        Assert.That(field[1, 0], Is.Null);
    }

    [Test]
    public void BattleShipTypesAreVisibleOnlyToBattleParticipants()
    {
        var field = Field2D.Create(2, 2);
        field[0, 0] = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, true, 0);
        field[1, 0] = KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipScissors, true, 0);

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            });

        var notification = rules.MakeMove(field, 1, move);
        var attackerNotification = rules.GetMoveNotificationForPlayer(notification, 1);
        var defenderNotification = rules.GetMoveNotificationForPlayer(notification, 2);
        var spectatorNotification = rules.GetMoveNotificationForPlayer(notification, 3);

        var attackerBattle = attackerNotification.MoveNotifications[0].Battle.Value;
        var defenderBattle = defenderNotification.MoveNotifications[0].Battle.Value;
        var spectatorBattle = spectatorNotification.MoveNotifications[0].Battle.Value;

        Assert.That(attackerBattle.attackerFigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipStone));
        Assert.That(attackerBattle.defenderFigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipScissors));
        Assert.That(defenderBattle.attackerFigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipStone));
        Assert.That(defenderBattle.defenderFigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipScissors));
        Assert.That(spectatorBattle.battleResult, Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.AttackerWon));
        Assert.That(spectatorBattle.attackerFigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.Unknown));
        Assert.That(spectatorBattle.defenderFigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.Unknown));
    }

    [Test]
    public void TurnRejectsMoreFiguresThanAllowedPerTurn()
    {
        var field = Field2D.Create(4, 3);
        var firstFigure = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, true, 0);
        var secondFigure = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipPaper, true, 0);
        var thirdFigure = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipScissors, true, 0);
        field[0, 0] = firstFigure;
        field[2, 1] = secondFigure;
        field[0, 2] = thirdFigure;

        var rules = new KaNoBuRules(6)
        {
            MaxMovesPerTurn = 2
        };

        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 0, Y = 1 }),
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 2, Y = 1 }, new Point { X = 2, Y = 2 }),
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 2 }, new Point { X = 1, Y = 2 }),
            });

        Assert.That(rules.IsMoveValid(field, 1, move), Is.EqualTo(false));
    }

    [Test]
    public void InitModelContainsMaxFiguresPerTurn()
    {
        var rules = new KaNoBuRules(6)
        {
            MaxMovesPerTurn = 4
        };

        var initModel = rules.GetInitModel(0);

        Assert.That(initModel.MaxMovesPerTurn, Is.EqualTo(4));
    }

    [Test]
    public void InitModelDistributesShipsAcrossFourMobileTypes()
    {
        var initModel = new KaNoBuRules(7).GetInitModel(0);

        Assert.That(initModel.AvailableFigures, Does.Contain(KaNoBuFigure.FigureTypes.ShipStone));
        Assert.That(initModel.AvailableFigures, Does.Contain(KaNoBuFigure.FigureTypes.ShipPaper));
        Assert.That(initModel.AvailableFigures, Does.Contain(KaNoBuFigure.FigureTypes.ShipScissors));
        Assert.That(initModel.AvailableFigures, Does.Contain(KaNoBuFigure.FigureTypes.ShipScout));
        Assert.That(initModel.AvailableFigures.FindAll(type => type == KaNoBuFigure.FigureTypes.ShipScout).Count, Is.EqualTo(1));
    }

    [Test]
    public void UniversalBecomesScoutWhenItBattlesScout()
    {
        var field = Field2D.Create(2, 1);
        field[0, 0] = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipScout, true, 0);
        field[1, 0] = KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipUniversal, true, 0);
        var rules = new KaNoBuRules(6);

        var notification = rules.MakeMove(field, 1, new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            }));

        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult, Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.DefenderWon));
        Assert.That(notification.MoveNotifications[0].Battle.Value.defenderFigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipScout));
        Assert.That(field[0, 0], Is.Null);
        Assert.That(((KaNoBuFigure)field[1, 0]).FigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipScout));
    }

    [Test]
    public void MultiMoveTurnAppliesStepsInOrder()
    {
        var field = Field2D.Create(4, 3);
        var firstFigure = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, true, 0);
        var secondFigure = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipPaper, true, 0);
        field[0, 0] = firstFigure;
        field[2, 1] = secondFigure;

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 0, Y = 1 }),
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 2, Y = 1 }, new Point { X = 2, Y = 2 }),
            });

        Assert.That(rules.IsMoveValid(field, 1, move), Is.EqualTo(true));

        var notification = rules.MakeMove(field, 1, move);

        Assert.That(notification.MoveNotifications.Count, Is.EqualTo(2));
        Assert.That(((KaNoBuFigure)field[0, 1]).FigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipStone));
        Assert.That(((KaNoBuFigure)field[2, 2]).FigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipPaper));
    }

    private sealed class FakeWinningFigure : KaNoBuFigure
    {
        private readonly KaNoBuFigure.FigureTypes _type;
        public FakeWinningFigure(int playerId, KaNoBuFigure.FigureTypes figureType, bool visibleForAllPlayers, int winNumber)
            : base(playerId, visibleForAllPlayers, winNumber)
        {
            _type = figureType;
        }

        public override KaNoBuFigure.FigureTypes FigureType => _type;

        public override bool IsMoveable => true;

        public override bool IsMoveValid(KaNoBuMoveResponseModel.MoveStep moveStep) => true;

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            return BattleResolution.AttackerWon(KaNoBuFigure.Create(this.PlayerId, KaNoBuFigure.FigureTypes.ShipStone, true, 0));
        }
    }
}
