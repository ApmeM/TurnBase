using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class KaNoBuMoveTests
{
    [TestCase(KaNoBuFigure.FigureTypes.ShipStone, true)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper, true)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors, true)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal, true)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout, true)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipFlag, false)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipMine, false)]
    [TestCase(KaNoBuFigure.FigureTypes.Unknown, false)]
    public void FigureMovementDependsOnType(KaNoBuFigure.FigureTypes figureType, bool expectedCanMove)
    {
        var figure = KaNoBuFigure.Create(1, figureType, 0);
        var field = Field2D.Create(5, 5);

        var adjacentMove = new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 1, Y = 2 });

        Assert.That(figure.IsMoveValid(adjacentMove, field), Is.EqualTo(expectedCanMove));
    }

    [TestCase(KaNoBuFigure.FigureTypes.ShipStone)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout)]
    public void MovableFigureRejectsNonAdjacentMove(KaNoBuFigure.FigureTypes figureType)
    {
        var figure = KaNoBuFigure.Create(1, figureType, 0);
        var field = Field2D.Create(5, 5);
        var nonAdjacentMove = new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 3, Y = 3 });

        Assert.That(figure.IsMoveValid(nonAdjacentMove, field), Is.False);
    }

    [Test]
    public void ScoutCanMoveUpToTwoManhattanCells()
    {
        var scout = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipScout, 0);
        var field = Field2D.Create(5, 5);

        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 3, Y = 1 }), field), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 1, Y = 3 }), field), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = -1, Y = 1 }), field), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 1, Y = -1 }), field), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 2, Y = 0 }), field), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 0, Y = 2 }), field), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 0, Y = 0 }), field), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 2, Y = 2 }), field), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 2, Y = 3 }), field), Is.False);
    }

    [TestCase(2, KaNoBuFigure.FigureTypes.ShipStone, false)]
    [TestCase(1, KaNoBuFigure.FigureTypes.ShipStone, true)]
    public void ScoutValidatesIntermediateFigureOwnership(
        int middlePlayerId,
        KaNoBuFigure.FigureTypes middleFigureType,
        bool expectedIsMoveValid)
    {
        var scout = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipScout, 0);
        var field = Field2D.Create(5, 3);
        field.SetFigure(2, 1, KaNoBuFigure.Create(middlePlayerId, middleFigureType, 0));
        var move = new KaNoBuMoveResponseModel.MoveStep(new Point(1, 1), new Point(3, 1));

        Assert.That(scout.IsMoveValid(move, field), Is.EqualTo(expectedIsMoveValid));
    }

    [TestCase(KaNoBuFigure.FigureTypes.ShipStone)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout)]
    public void FigureCannotMoveToSamePosition(KaNoBuFigure.FigureTypes figureType)
    {
        var figure = KaNoBuFigure.Create(1, figureType, 0);
        var field = Field2D.Create(5, 5);
        var samePositionMove = new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 1, Y = 1 });

        Assert.That(figure.IsMoveValid(samePositionMove, field), Is.False);
    }
}
