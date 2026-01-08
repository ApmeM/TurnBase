using System.Collections.Generic;

namespace TurnBase.KaNoBu
{
    public class KaNoBuInitModel
    {
        public KaNoBuInitModel(int width, int height, List<KaNoBuFigure.FigureTypes> availableFigures)
        {
            Width = width;
            Height = height;
            AvailableFigures = availableFigures;
        }

        public readonly List<KaNoBuFigure.FigureTypes> AvailableFigures;
        public readonly int Width;
        public readonly int Height;
    }

    public class KaNoBuInitResponseModel
    {
        public KaNoBuInitResponseModel(KaNoBuFigure.FigureTypes[,] preparedField)
        {
            PreparedField = preparedField;
        }

        public readonly KaNoBuFigure.FigureTypes[,] PreparedField;
    }

    public class KaNoBuMoveModel
    {
        public struct FigureModel
        {
            public KaNoBuFigure.FigureTypes FigureType;
            public int PlayerNumber;
        }

        public KaNoBuMoveModel(FigureModel?[,] field)
        {
            Field = field;
        }

        public readonly FigureModel?[,] Field;
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
        public struct Battle
        {
            public BattleResult battleResult;
            public bool isDefenderFlag;
        }

        public enum BattleResult
        {
            Draw,
            AttackerWon,
            DefenderWon
        }

        public KaNoBuMoveNotificationModel(KaNoBuMoveResponseModel move, BattleResult battleResult, bool isDefenderFlag)
        {
            this.move = move;
            this.battle = new Battle { battleResult = battleResult, isDefenderFlag = isDefenderFlag };
        }

        public KaNoBuMoveNotificationModel(KaNoBuMoveResponseModel move)
        {
            this.move = move;
            this.battle = null;
        }

        public readonly KaNoBuMoveResponseModel move;
        public readonly Battle? battle;
    }
}