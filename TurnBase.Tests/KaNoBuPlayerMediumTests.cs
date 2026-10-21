using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class KaNoBuPlayerMediumTests
{
    [Test]
    public async Task MediumBotSkipsWallDestinations()
    {
        var player = new KaNoBuPlayerMedium();
        var field = Field2D.Create(2, 2);
        field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.SetWall(0, 1, true);
        field.SetWall(1, 0, true);

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        Assert.That(result.Response.Moves.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task MediumBotUsesMoveLimitWithoutMovingAShipTwice()
    {
        var player = new KaNoBuPlayerMedium();
        await Initialize(player, 2);
        var field = CreateFieldWithThreeMovableShips();

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        Assert.That(result.Response.Moves.Count, Is.EqualTo(2));
        Assert.That(result.Response.Moves.Select(move => move.From).Distinct().Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task MediumBotDoesNotAttackKnownEnemyWithScout()
    {
        var player = new KaNoBuPlayerMedium();
        await Initialize(player, 1);
        var field = Field2D.Create(3, 2);
        field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipScout, 0));
        field.SetFigure(1, 0, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, 0));

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        Assert.That(result.Response.Moves, Is.Not.Empty);
        Assert.That(result.Response.Moves[0].To, Is.Not.EqualTo(new Point(1, 0)));
    }

    [Test]
    public async Task MediumBotAttacksKnownFlagWithScout()
    {
        var player = new KaNoBuPlayerMedium();
        await Initialize(player, 1);
        var field = Field2D.Create(3, 2);
        var flagPosition = new Point(1, 0);
        field.SetFigure(new Point(0, 0), KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipScout, 0));
        field.SetFigure(flagPosition, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipFlag, 0));

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        Assert.That(result.Response.Moves[0].To, Is.EqualTo(flagPosition));
    }

    [Test]
    public async Task MediumBotPrioritizesScoutReconnaissanceOverRegularUnknownAttack()
    {
        var player = new KaNoBuPlayerMedium();
        await Initialize(player, 1);
        var field = Field2D.Create(7, 3);
        var scoutPosition = new Point(0, 0);
        var scoutTarget = new Point(1, 0);
        var regularShipPosition = new Point(5, 0);
        field.SetFigure(scoutPosition, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipScout, 0));
        field.SetFigure(scoutTarget, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.Unknown, 0));
        field.SetFigure(regularShipPosition, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.SetFigure(new Point(6, 0), KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.Unknown, 0));

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        Assert.That(result.Response.Moves[0].From, Is.EqualTo(scoutPosition));
        Assert.That(result.Response.Moves[0].To, Is.EqualTo(scoutTarget));
    }

    [TestCase(KaNoBuFigure.FigureTypes.ShipMine)]
    [TestCase(KaNoBuFigure.FigureTypes.ShipUniversal)]
    public async Task MediumBotDoesNotAttackKnownNonWinningMatchupsWithUniversal(
        KaNoBuFigure.FigureTypes defenderType)
    {
        var player = new KaNoBuPlayerMedium();
        await Initialize(player, 1);
        var field = Field2D.Create(3, 2);
        var enemyPosition = new Point(1, 0);
        field.SetFigure(new Point(0, 0), KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipUniversal, 0));
        field.SetFigure(enemyPosition, KaNoBuFigure.Create(1, defenderType, 0));

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        Assert.That(result.Response.Moves[0].To, Is.Not.EqualTo(enemyPosition));
    }

    [Test]
    public async Task MediumBotRetreatsFromAdjacentKnownShipThatWouldWin()
    {
        var player = new KaNoBuPlayerMedium();
        await Initialize(player, 1);
        var field = Field2D.Create(4, 3);
        var shipPosition = new Point(1, 1);
        var enemyPosition = new Point(2, 1);
        field.SetFigure(shipPosition, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.SetFigure(enemyPosition, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipPaper, 0));

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        var destination = result.Response.Moves[0].To;
        Assert.That(ManhattanDistance(destination, enemyPosition), Is.GreaterThan(ManhattanDistance(shipPosition, enemyPosition)));
    }

    [Test]
    public async Task MediumBotMovesScoutAwayFromNearbyKnownEnemy()
    {
        var player = new KaNoBuPlayerMedium();
        await Initialize(player, 1);
        var field = Field2D.Create(6, 6);
        var scoutPosition = new Point(2, 2);
        var enemyPosition = new Point(2, 4);
        field.SetFigure(scoutPosition, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipScout, 0));
        field.SetFigure(enemyPosition, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.ShipStone, 0));

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        Assert.That(result.Response.Moves, Is.Not.Empty);
        var destination = result.Response.Moves[0].To;
        Assert.That(ManhattanDistance(destination, enemyPosition), Is.GreaterThan(ManhattanDistance(scoutPosition, enemyPosition)));
    }

    [Test]
    public async Task MediumBotMovesScoutTowardUnknownEnemyAsLowPriorityGoal()
    {
        var player = new KaNoBuPlayerMedium();
        await Initialize(player, 1);
        var field = Field2D.Create(7, 3);
        var scoutPosition = new Point(0, 1);
        var unknownEnemyPosition = new Point(5, 1);
        field.SetFigure(scoutPosition, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipScout, 0));
        field.SetFigure(unknownEnemyPosition, KaNoBuFigure.Create(1, KaNoBuFigure.FigureTypes.Unknown, 0));

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(new KaNoBuField { Field = field })
        });

        Assert.That(result.Response.Moves, Is.Not.Empty);
        var destination = result.Response.Moves[0].To;
        Assert.That(ManhattanDistance(destination, unknownEnemyPosition), Is.LessThan(ManhattanDistance(scoutPosition, unknownEnemyPosition)));
    }

    private static int ManhattanDistance(Point first, Point second)
    {
        return System.Math.Abs(first.X - second.X) + System.Math.Abs(first.Y - second.Y);
    }

    private static async Task Initialize(KaNoBuPlayerMedium player, int maxMovesPerTurn)
    {
        await player.Init(new InitModel<KaNoBuInitModel>
        {
            PlayerId = 0,
            Request = new KaNoBuInitModel(1, 1,
                new List<KaNoBuFigure.FigureTypes> { KaNoBuFigure.FigureTypes.ShipStone }, maxMovesPerTurn)
        });
    }

    private static Field2D CreateFieldWithThreeMovableShips()
    {
        var field = Field2D.Create(7, 2);
        field.SetFigure(0, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0));
        field.SetFigure(3, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipPaper, 0));
        field.SetFigure(6, 0, KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipScissors, 0));
        return field;
    }
}
