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
        public KaNoBuMoveResponseModel(Point from, Point to)
        {
            Status = MoveStatus.MAKE_TURN;
            From = from;
            To = to;
        }

        public KaNoBuMoveResponseModel()
        {
            Status = MoveStatus.SKIP_TURN;
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
        public enum BattleResult {
            Draw,
            AttackerWon,
            DefenderWon
        }

        public KaNoBuMoveNotificationModel(KaNoBuMoveResponseModel move, BattleResult attackerWon, bool isDefenderFlag)
        {
            this.move = move;
            this.battle = (attackerWon, isDefenderFlag);
        }

        public KaNoBuMoveNotificationModel(KaNoBuMoveResponseModel move)
        {
            this.move = move;
            this.battle = null;
        }

        public readonly KaNoBuMoveResponseModel move;
        public readonly (BattleResult, bool)? battle;
    }
}