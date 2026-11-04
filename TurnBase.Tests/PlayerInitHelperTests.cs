using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TurnBase.KaNoBu;

[TestFixture]
public class PlayerInitHelperTests
{
    [Test]
    public void CreateRandomFieldPlacesEveryAvailableFigureAndConsumesTheList()
    {
        var availableFigures = new List<KaNoBuFigure.FigureTypes>
        {
            KaNoBuFigure.FigureTypes.ShipFlag,
            KaNoBuFigure.FigureTypes.ShipMine,
            KaNoBuFigure.FigureTypes.ShipScout,
            KaNoBuFigure.FigureTypes.ShipScout
        };
        var model = new KaNoBuInitModel(2, 2, availableFigures, int.MaxValue);

        var field = PlayerInitHelper.CreateRandomField(3, model);

        var placedFigures = Enumerable.Range(0, field.Width)
            .SelectMany(x => Enumerable.Range(0, field.Height)
                .Select(y => (KaNoBuFigure)field.GetFigure(x, y)))
            .ToList();
        Assert.That(placedFigures, Has.Count.EqualTo(4));
        Assert.That(placedFigures.All(figure => figure.PlayerId == 3), Is.True);
        Assert.That(placedFigures.Select(figure => figure.FigureType), Is.EquivalentTo(new[]
        {
            KaNoBuFigure.FigureTypes.ShipFlag,
            KaNoBuFigure.FigureTypes.ShipMine,
            KaNoBuFigure.FigureTypes.ShipScout,
            KaNoBuFigure.FigureTypes.ShipScout
        }));
        Assert.That(availableFigures, Is.Empty);
    }
}