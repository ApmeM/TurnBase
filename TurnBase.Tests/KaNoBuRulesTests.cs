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
        var field = new KaNoBuField
        {
            Field = Field2D.Create(2, 2)
        };
        var attacker = new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipStone, 2, 2);
        var defender = KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipPaper, 0);
        field.Field.SetFigure(0, 0, attacker);
        field.Field.SetFigure(1, 0, defender);

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            });

        var notification = rules.TryApplyMoveResponse(field, 1, move);

        Assert.That(notification.MoveNotifications.Count, Is.EqualTo(1));
        Assert.That(notification.MoveNotifications[0].Battle, Is.Not.Null);
        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult, Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.AttackerWon));

        var placed = (KaNoBuFigure)field.Field.GetFigure(1, 0);
        Assert.That(placed, Is.Not.Null);
        Assert.That(placed.FigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipUniversal));
        Assert.That(placed.PlayerId, Is.EqualTo(1));
    }

    [Test]
    public void BattleResultUsesPlayerIdentityWhenWinnerInstanceChanges()
    {
        var field = new KaNoBuField
        {
            Field = Field2D.Create(2, 2)
        };
        var attacker = new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipUniversal, 0);
        var defender = KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipStone, 0);
        field.Field.SetFigure(0, 0, attacker);
        field.Field.SetFigure(1, 0, defender);

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            });

        var notification = rules.TryApplyMoveResponse(field, 1, move);

        Assert.That(notification.MoveNotifications.Count, Is.EqualTo(1));
        Assert.That(notification.MoveNotifications[0].Battle, Is.Not.Null);
        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult, Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.AttackerWon));
    }

    [Test]
    public void BothDestroyedBattleRemovesBothFigures()
    {
        var field = new KaNoBuField
        {
            Field = Field2D.Create(2, 2)
        };
        field.Field.SetFigure(0, 0, new FakeBothDestroyedFigure(1));
        field.Field.SetFigure(1, 0, KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipPaper, 0));

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            });

        var notification = rules.TryApplyMoveResponse(field, 1, move);

        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult, Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.BothDestroyed));
        Assert.That(field.Field.GetFigure(0, 0), Is.Null);
        Assert.That(field.Field.GetFigure(1, 0), Is.Null);
    }

    [Test]
    public void FlagCapturedResultTransfersDefeatedPlayerFleetToAttacker()
    {
        var field = new KaNoBuField
        {
            Field = Field2D.Create(3, 2)
        };
        field.Field.SetFigure(0, 0, new FakeFlagCaptureFigure(1));
        field.Field.SetFigure(1, 0, KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.Field.SetFigure(0, 1, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipPaper, 0));
        field.Field.SetFigure(1, 1, KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipScissors, 0));
        field.Field.SetFigure(2, 1, KaNoBuFigure.Create(3, KaNoBuFigure.FigureTypes.ShipScout, 0));

        var rules = new KaNoBuRules(6);
        var notification = rules.TryApplyMoveResponse(field, 1, new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            }));

        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult,
            Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.FlagCaptured));
        Assert.That(((KaNoBuFigure)field.Field.GetFigure(0, 1)).PlayerId, Is.EqualTo(1));
        Assert.That(((KaNoBuFigure)field.Field.GetFigure(1, 1)).PlayerId, Is.EqualTo(1));
        Assert.That(((KaNoBuFigure)field.Field.GetFigure(2, 1)).PlayerId, Is.EqualTo(3));
    }

    [Test]
    public void BattleShipTypesAreVisibleOnlyToBattleParticipants()
    {
        var field = new KaNoBuField
        {
            Field = Field2D.Create(2, 2)
        };
        field.Field.SetFigure(0, 0, new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.Field.SetFigure(1, 0, KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipScissors, 0));

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            });

        var notification = rules.TryApplyMoveResponse(field, 1, move);
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
    public void ScoutedBattleLeavesBothFiguresInPlace()
    {
        var field = new KaNoBuField { Field = Field2D.Create(2, 1) };
        var attacker = new FakeScoutedFigure(1);
        var defender = KaNoBuFigure.Create(2, KaNoBuFigure.FigureTypes.ShipMine, 0);
        field.Field.SetFigure(0, 0, attacker);
        field.Field.SetFigure(1, 0, defender);

        var notification = new KaNoBuRules(6).TryApplyMoveResponse(field, 1, new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 1, Y = 0 })
            }));

        Assert.That(notification.MoveNotifications[0].Battle.Value.battleResult,
            Is.EqualTo(KaNoBuMoveNotificationModel.BattleResult.Scouted));
        Assert.That(field.Field.GetFigure(0, 0), Is.SameAs(attacker));
        Assert.That(field.Field.GetFigure(1, 0), Is.SameAs(defender));
        Assert.That(attacker.WinNumber, Is.Zero);
    }

    [Test]
    public void TurnRejectsMoreFiguresThanAllowedPerTurn()
    {
        var field = new KaNoBuField
        {
            Field = Field2D.Create(4, 3)
        };
        var firstFigure = new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipStone, 0);
        var secondFigure = new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipPaper, 0);
        var thirdFigure = new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipScissors, 0);
        field.Field.SetFigure(0, 0, firstFigure);
        field.Field.SetFigure(2, 1, secondFigure);
        field.Field.SetFigure(0, 2, thirdFigure);

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

        var initModel = rules.GetInitModelForPlayer(0);

        Assert.That(initModel.MaxMovesPerTurn, Is.EqualTo(4));
    }

    [Test]
    [TestCase(6, 6)]
    [TestCase(7, 7)]
    [TestCase(8, 8)]
    [TestCase(9, 9)]
    [TestCase(10, 10)]
    [TestCase(10, 8)]
    [TestCase(8, 10)]
    public void InitResponsePlacesLastRowClosestToEachPlayersEdge(int fieldWidth, int fieldHeight)
    {
        var rules = new KaNoBuRules(Field2D.Create(fieldWidth, fieldHeight));
        var gameField = rules.GenerateGameField();
        var initFieldHeight = fieldHeight / 3;

        for (var playerNumber = 0; playerNumber < 4; playerNumber++)
        {
            var initModel = rules.GetInitModelForPlayer(playerNumber);
            var preparedField = Field2D.Create(initModel.Width, initModel.Height);
            var figureIndex = 0;
            for (var i = 0; i < initModel.Width; i++)
            {
                for (var j = 0; j < initModel.Height; j++)
                {
                    preparedField.SetFigure(i, j, KaNoBuFigure.Create(
                        playerNumber,
                        initModel.AvailableFigures[figureIndex++],
                        0));
                }
            }

            Assert.That(rules.TryApplyInitResponse(
                gameField,
                playerNumber,
                new KaNoBuInitResponseModel(preparedField)), Is.True);

            for (var i = 0; i < initModel.Width; i++)
            {
                for (var j = 0; j < initModel.Height; j++)
                {
                    Point expectedPosition;
                    if (playerNumber == 0)
                    {
                        expectedPosition = new Point(
                            (fieldWidth - initModel.Width) / 2 + i,
                            fieldHeight - initModel.Height + j);
                    }
                    else if (playerNumber == 1)
                    {
                        expectedPosition = new Point(
                            fieldWidth - (fieldWidth - initModel.Width) / 2 - 1 - i,
                            initFieldHeight - 1 - j);
                    }
                    else if (playerNumber == 2)
                    {
                        expectedPosition = new Point(
                            fieldWidth - initModel.Height + j,
                            fieldHeight - (fieldHeight - initModel.Width) / 2 - 1 - i);
                    }
                    else
                    {
                        expectedPosition = new Point(
                            initModel.Height - 1 - j,
                            (fieldHeight - initModel.Width) / 2 + i);
                    }

                    var expectedFigure = preparedField.GetFigure(i, j) as KaNoBuFigure;
                    Assert.That(gameField.Field.IsInBounds(expectedPosition), Is.True);
                    var placedFigure = gameField.Field.GetFigure(expectedPosition) as KaNoBuFigure;
                    Assert.That(placedFigure, Is.Not.Null);
                    Assert.That(placedFigure.PlayerId, Is.EqualTo(playerNumber));
                    Assert.That(placedFigure.FigureType, Is.EqualTo(expectedFigure.FigureType));
                }
            }
        }
    }

    [Test]
    public void InitModelDistributesShipsAcrossFourMobileTypes()
    {
        var initModel = new KaNoBuRules(7).GetInitModelForPlayer(0);

        Assert.That(initModel.AvailableFigures, Does.Contain(KaNoBuFigure.FigureTypes.ShipStone));
        Assert.That(initModel.AvailableFigures, Does.Contain(KaNoBuFigure.FigureTypes.ShipPaper));
        Assert.That(initModel.AvailableFigures, Does.Contain(KaNoBuFigure.FigureTypes.ShipScissors));
        Assert.That(initModel.AvailableFigures, Does.Contain(KaNoBuFigure.FigureTypes.ShipScout));
        Assert.That(initModel.AvailableFigures.FindAll(type => type == KaNoBuFigure.FigureTypes.ShipScout).Count, Is.EqualTo(1));
    }

    [Test]
    public void MultiMoveTurnAppliesStepsInOrder()
    {
        var field = new KaNoBuField
        {
            Field = Field2D.Create(4, 3)
        };
        var firstFigure = new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipStone, 0);
        var secondFigure = new FakeWinningFigure(1, KaNoBuFigure.FigureTypes.ShipPaper, 0);
        field.Field.SetFigure(0, 0, firstFigure);
        field.Field.SetFigure(2, 1, secondFigure);

        var rules = new KaNoBuRules(6);
        var move = new KaNoBuMoveResponseModel(
            new List<KaNoBuMoveResponseModel.MoveStep>
            {
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 0, Y = 0 }, new Point { X = 0, Y = 1 }),
                new KaNoBuMoveResponseModel.MoveStep(new Point { X = 2, Y = 1 }, new Point { X = 2, Y = 2 }),
            });

        Assert.That(rules.IsMoveValid(field, 1, move), Is.EqualTo(true));

        var notification = rules.TryApplyMoveResponse(field, 1, move);

        Assert.That(notification.MoveNotifications.Count, Is.EqualTo(2));
        Assert.That(((KaNoBuFigure)field.Field.GetFigure(0, 1)).FigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipStone));
        Assert.That(((KaNoBuFigure)field.Field.GetFigure(2, 2)).FigureType, Is.EqualTo(KaNoBuFigure.FigureTypes.ShipPaper));
    }

    private sealed class FakeWinningFigure : KaNoBuFigure
    {
        private readonly KaNoBuFigure.FigureTypes _type;
        private readonly int _winnerWinNumber;

        public FakeWinningFigure(
            int playerId,
            KaNoBuFigure.FigureTypes figureType,
            int winNumber,
            int winnerWinNumber = 0)
            : base(playerId, winNumber)
        {
            _type = figureType;
            _winnerWinNumber = winnerWinNumber;
        }

        public override KaNoBuFigure.FigureTypes FigureType => _type;

        public override bool IsMoveable => true;

        public override Point[] GetPossibleMoveOffsets()
        {
            return new[]
            {
                new Point(0, 1),
            };
        }

        public override bool IsMoveValid(KaNoBuMoveResponseModel.MoveStep moveStep, Field2D mainField) => true;

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            return BattleResolution.AttackerWon(
                KaNoBuFigure.Create(this.PlayerId, KaNoBuFigure.FigureTypes.ShipStone, _winnerWinNumber));
        }
    }

    private sealed class FakeBothDestroyedFigure : KaNoBuFigure
    {
        public FakeBothDestroyedFigure(int playerId)
            : base(playerId, 0)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipStone;

        public override bool IsMoveable => true;

        public override Point[] GetPossibleMoveOffsets() => new[] { new Point(1, 0) };

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            return BattleResolution.BothAreDestroyed();
        }
    }

    private sealed class FakeScoutedFigure : KaNoBuFigure
    {
        public FakeScoutedFigure(int playerId)
            : base(playerId, 0)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipScout;

        public override bool IsMoveable => true;

        public override Point[] GetPossibleMoveOffsets() => new[] { new Point(1, 0) };

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            return BattleResolution.Scouted();
        }
    }

    private sealed class FakeFlagCaptureFigure : KaNoBuFigure
    {
        public FakeFlagCaptureFigure(int playerId)
            : base(playerId, 0)
        {
        }

        public override KaNoBuFigure.FigureTypes FigureType => KaNoBuFigure.FigureTypes.ShipUniversal;

        public override bool IsMoveable => true;

        public override Point[] GetPossibleMoveOffsets() => new[] { new Point(1, 0) };

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            return BattleResolution.FlagCaptured(this);
        }
    }
}
