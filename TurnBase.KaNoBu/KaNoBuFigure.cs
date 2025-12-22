using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class KaNoBuFigure : IFigure
{
    public enum FigureTypes
    {
        ShipFlag = 1,
        ShipStone = 2,
        ShipPaper = 3,
        ShipScissors = 4
    }

    public int PlayerId { get; private set; }
    public FigureTypes FigureType { get; private set; }

    public KaNoBuFigure(int playerId, FigureTypes figure)
    {
        PlayerId = playerId;
        FigureType = figure;
    }
}