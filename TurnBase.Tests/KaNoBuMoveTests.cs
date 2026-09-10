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
        var figure = KaNoBuFigure.Create(1, figureType, true, 0);

        var adjacentMove = new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 1, Y = 2 });

        Assert.That(figure.IsMoveValid(adjacentMove), Is.EqualTo(expectedCanMove));
    }

    [TestCase(KaNoBuFigure.FigureTypes.ShipStone)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout)]
    public void MovableFigureRejectsNonAdjacentMove(KaNoBuFigure.FigureTypes figureType)
    {
        var figure = KaNoBuFigure.Create(1, figureType, true, 0);
        var nonAdjacentMove = new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 3, Y = 3 });

        Assert.That(figure.IsMoveValid(nonAdjacentMove), Is.False);
    }

    [Test]
    public void ScoutCanMoveUpToTwoManhattanCells()
    {
        var scout = KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipScout, true, 0);

        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 3, Y = 1 })), Is.True);
        Assert.That(scout.IsMoveValid(new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 2, Y = 3 })), Is.False);
    }

    [TestCase(KaNoBuFigure.FigureTypes.ShipStone)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipPaper)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScissors)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipScout)]
    public void FigureCannotMoveToSamePosition(KaNoBuFigure.FigureTypes figureType)
    {
        var figure = KaNoBuFigure.Create(1, figureType, true, 0);
        var samePositionMove = new KaNoBuMoveResponseModel.MoveStep(
            new Point { X = 1, Y = 1 },
            new Point { X = 1, Y = 1 });

        Assert.That(figure.IsMoveValid(samePositionMove), Is.False);
    }
}
