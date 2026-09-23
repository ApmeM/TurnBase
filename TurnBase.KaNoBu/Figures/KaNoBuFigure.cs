using System;

namespace TurnBase.KaNoBu
{
    public abstract class KaNoBuFigure : IFigure
    {
        public enum FigureTypes
        {
            Unknown,
            ShipFlag,
            ShipStone,
            ShipPaper,
            ShipScissors,
            ShipUniversal,
            ShipMine,
            ShipScout,
        }

        protected KaNoBuFigure(int playerId, int winNumber)
        {
            this.PlayerId = playerId;
            this.WinNumber = winNumber;
        }

        public int PlayerId { get; set; }
        public abstract FigureTypes FigureType { get; }
        public int WinNumber { get; set; }

        public KaNoBuFigure WithFigureType(FigureTypes figureType)
        {
            return Create(this.PlayerId, figureType, this.WinNumber);
        }

        public static KaNoBuFigure Create(int playerId, FigureTypes figureType, int winNumber)
        {
            switch (figureType)
            {
                case FigureTypes.Unknown:
                    return new UnknownKaNoBuFigure(playerId, winNumber);
                case FigureTypes.ShipFlag:
                    return new ShipFlagKaNoBuFigure(playerId, winNumber);
                case FigureTypes.ShipStone:
                    return new ShipStoneKaNoBuFigure(playerId, winNumber);
                case FigureTypes.ShipPaper:
                    return new ShipPaperKaNoBuFigure(playerId, winNumber);
                case FigureTypes.ShipScissors:
                    return new ShipScissorsKaNoBuFigure(playerId, winNumber);
                case FigureTypes.ShipUniversal:
                    return new ShipUniversalKaNoBuFigure(playerId, winNumber);
                case FigureTypes.ShipMine:
                    return new ShipMineKaNoBuFigure(playerId, winNumber);
                case FigureTypes.ShipScout:
                    return new ShipScoutKaNoBuFigure(playerId, winNumber);
                default:
                    throw new Exception("Unknown figure type");
            }
        }

        public abstract bool IsMoveable { get; }

        public virtual bool IsMoveValid(KaNoBuMoveResponseModel.MoveStep moveStep)
        {
            var offset = new Point(
                moveStep.To.X - moveStep.From.X,
                moveStep.To.Y - moveStep.From.Y);

            foreach (var possibleOffset in this.GetPossibleMoveOffsets())
            {
                if (possibleOffset.X == offset.X && possibleOffset.Y == offset.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract Point[] GetPossibleMoveOffsets();

        public abstract BattleResolution ResolveBattle(KaNoBuFigure defender);

        protected static FigureTypes GetTypeThatDefeats(FigureTypes defenderType)
        {
            switch (defenderType)
            {
                case FigureTypes.ShipPaper:
                    return FigureTypes.ShipScissors;
                case FigureTypes.ShipScissors:
                    return FigureTypes.ShipStone;
                case FigureTypes.ShipStone:
                    return FigureTypes.ShipPaper;
                default:
                    return defenderType;
            }
        }

        public IFigure Clone()
        {
            return Create(this.PlayerId, this.FigureType, this.WinNumber);
        }

        public override string ToString()
        {
            return this.PlayerId + this.FigureType.PrintableName();
        }
    }
}
