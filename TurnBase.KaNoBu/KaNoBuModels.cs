using System.Collections.Generic;
using TurnBase;

namespace TurnBase.KaNoBu
{
    public class KaNoBuInitModel
    {
        public KaNoBuInitModel(int width, int height, List<IFigure> availableFigures)
        {
            Width = width;
            Height = height;
            AvailableFigures = availableFigures;
        }

        public readonly List<IFigure> AvailableFigures;
        public readonly int Width;
        public readonly int Height;
    }

    public class KaNoBuInitResponseModel
    {
        public KaNoBuInitResponseModel(IField preparedField)
        {
            PreparedField = preparedField;
        }

        public readonly IField PreparedField;
    }

    public class KaNoBuMoveModel
    {
        public KaNoBuMoveModel(IField field)
        {
            Field = field;
        }

        public readonly IField Field;
    }

    public class KaNoBuMoveResponseModel
    {
        public KaNoBuMoveResponseModel(MoveStatus status, Point from, Point to)
        {
            Status = status;
            From = from;
            To = to;
        }

        public KaNoBuMoveResponseModel(MoveStatus status)
        {
            Status = status;
            From = new Point();
            To = new Point();
        }

        public enum MoveStatus
        {
            MAKE_TURN,
            SKIP_TURN
        }

        public readonly MoveStatus Status;
        public readonly Point From;
        public readonly Point To;
    }

    public class KaNoBuMoveNotificationModel
    {
        public KaNoBuMoveNotificationModel(KaNoBuMoveResponseModel move, IFigure attacker, IFigure defender, IFigure winner)
        {
            this.move = move;
            this.battle = (attacker, defender, winner);
        }

        public KaNoBuMoveNotificationModel(KaNoBuMoveResponseModel move)
        {
            this.move = move;
            this.battle = null;
        }

        public readonly KaNoBuMoveResponseModel move;
        public readonly (IFigure, IFigure, IFigure)? battle;
    }
}