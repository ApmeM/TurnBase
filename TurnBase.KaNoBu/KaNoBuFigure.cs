using System;
using TurnBase;

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
                default:
                    throw new Exception("Unknown figure type");
            }
        }
    }

    public class KaNoBuFigure : IFigure
    {
        private readonly bool visibleForAllPlayers;

        public enum FigureTypes
        {
            Unknown = 0,
            ShipFlag = 1,
            ShipStone = 2,
            ShipPaper = 3,
            ShipScissors = 4,
            ShipUniversal = 5
        }

        public int PlayerId { get; set; }
        public FigureTypes FigureType { get; set; }
        public int WinNumber { get; set; }

        public KaNoBuFigure(int playerId, FigureTypes figureType, bool visibleForAllPlayers, int winNumber)
        {
            PlayerId = playerId;
            FigureType = figureType;
            this.visibleForAllPlayers = visibleForAllPlayers;
            WinNumber = winNumber;
        }

        public bool IsMoveValid(KaNoBuMoveResponseModel playerMove)
        {
            if (this.FigureType == FigureTypes.ShipFlag || this.FigureType == FigureTypes.Unknown)
            {
                return false;
            }

            var validMove =
                (playerMove.From.X == playerMove.To.X && playerMove.From.Y <= playerMove.To.Y + 1 && playerMove.From.Y >= playerMove.To.Y - 1) ||
                (playerMove.From.Y == playerMove.To.Y && playerMove.From.X <= playerMove.To.X + 1 && playerMove.From.X >= playerMove.To.X - 1);
            if (!validMove)
            {
                return false;
            }

            return true;
        }

        public IFigure CopyForPlayer(int playerId)
        {
            if (this.PlayerId == playerId || playerId == -1 || visibleForAllPlayers)
            {
                return new KaNoBuFigure(this.PlayerId, this.FigureType, visibleForAllPlayers, this.WinNumber);
            }
            else
            {
                return new KaNoBuFigure(this.PlayerId, FigureTypes.Unknown, visibleForAllPlayers, this.WinNumber);
            }
        }

        public override string ToString()
        {
            return this.PlayerId + this.FigureType.PrintableName();
        }
    }
}