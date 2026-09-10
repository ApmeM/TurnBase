using System;

namespace TurnBase.KaNoBu
{
    public static class FigureTypeInfo
    {
        public static string PrintableName(this KaNoBuFigure.FigureTypes figureType)
        {
            switch (figureType)
            {
                case KaNoBuFigure.FigureTypes.Unknown:
                    return "?";
                case KaNoBuFigure.FigureTypes.ShipFlag:
                    return "F";
                case KaNoBuFigure.FigureTypes.ShipStone:
                    return "R";
                case KaNoBuFigure.FigureTypes.ShipPaper:
                    return "P";
                case KaNoBuFigure.FigureTypes.ShipScissors:
                    return "S";
                case KaNoBuFigure.FigureTypes.ShipUniversal:
                    return "U";
                case KaNoBuFigure.FigureTypes.ShipMine:
                    return "M";
                case KaNoBuFigure.FigureTypes.ShipScout:
                    return "C";
                default:
                    throw new Exception("Unknown figure type");
            }
        }
    }
}
