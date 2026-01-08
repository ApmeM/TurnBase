using TurnBase;

namespace TurnBase.KaNoBu
{
    public class KaNoBuFigure : IFigure
    {
        public enum FigureTypes
        {
            Unknown = 0,
            ShipFlag = 1,
            ShipStone = 2,
            ShipPaper = 3,
            ShipScissors = 4
        }

        public int PlayerId { get; set; }
        public FigureTypes FigureType { get; private set; }

        public KaNoBuFigure(int playerId, FigureTypes figure)
        {
            PlayerId = playerId;
            FigureType = figure;
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
    }
}