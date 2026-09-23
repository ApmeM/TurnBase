using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TurnBase;
using TurnBase.KaNoBu;

[TestFixture]
public class KaNoBuPlayerEasyTests
{
    [Test]
    public async Task EasyBotSkipsWallDestinations()
    {
        var player = new KaNoBuPlayerEasy();
        var field = Field2D.Create(2, 2);
        field[0, 0] = KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0);
        field.walls[0, 1] = true;
        field.walls[1, 0] = true;

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(field)
        });

        Assert.That(result.Response.Moves.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task EasyBotUsesMoveLimitWithoutMovingAShipTwice()
    {
        var player = new KaNoBuPlayerEasy();
        await Initialize(player, 2);
        var field = CreateFieldWithThreeMovableShips();

        var result = await player.MakeTurn(new MakeTurnModel<KaNoBuMoveModel>
        {
            Request = new KaNoBuMoveModel(field)
        });

        Assert.That(result.Response.Moves.Count, Is.EqualTo(2));
        Assert.That(result.Response.Moves.Select(move => move.From).Distinct().Count(), Is.EqualTo(2));
    }

    private static async Task Initialize(KaNoBuPlayerEasy player, int maxMovesPerTurn)
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
        field[0, 0] = KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipStone, 0);
        field[3, 0] = KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipPaper, 0);
        field[6, 0] = KaNoBuFigure.Create(0, KaNoBuFigure.FigureTypes.ShipScissors, 0);
        return field;
    }
}
