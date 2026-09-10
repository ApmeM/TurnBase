using System;
using TurnBase;

namespace TurnBase.KaNoBu
{
    public abstract class KaNoBuFigure : IFigure
    {
        private readonly bool visibleForAllPlayers;

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

        protected KaNoBuFigure(int playerId, bool visibleForAllPlayers, int winNumber)
        {
            PlayerId = playerId;
            this.visibleForAllPlayers = visibleForAllPlayers;
            WinNumber = winNumber;
        }

        public int PlayerId { get; set; }
        public abstract FigureTypes FigureType { get; }
        public int WinNumber { get; set; }

        public bool VisibleForAllPlayers => this.visibleForAllPlayers;

        public KaNoBuFigure WithFigureType(FigureTypes figureType)
        {
            return Create(this.PlayerId, figureType, this.VisibleForAllPlayers, this.WinNumber);
        }

        public static KaNoBuFigure Create(int playerId, FigureTypes figureType, bool visibleForAllPlayers, int winNumber)
        {
            switch (figureType)
            {
                case FigureTypes.Unknown:
                    return new UnknownKaNoBuFigure(playerId, visibleForAllPlayers, winNumber);
                case FigureTypes.ShipFlag:
                    return new ShipFlagKaNoBuFigure(playerId, visibleForAllPlayers, winNumber);
                case FigureTypes.ShipStone:
                    return new ShipStoneKaNoBuFigure(playerId, visibleForAllPlayers, winNumber);
                case FigureTypes.ShipPaper:
                    return new ShipPaperKaNoBuFigure(playerId, visibleForAllPlayers, winNumber);
                case FigureTypes.ShipScissors:
                    return new ShipScissorsKaNoBuFigure(playerId, visibleForAllPlayers, winNumber);
                case FigureTypes.ShipUniversal:
                    return new ShipUniversalKaNoBuFigure(playerId, visibleForAllPlayers, winNumber);
                case FigureTypes.ShipMine:
                    return new ShipMineKaNoBuFigure(playerId, visibleForAllPlayers, winNumber);
                case FigureTypes.ShipScout:
                    return new ShipScoutKaNoBuFigure(playerId, visibleForAllPlayers, winNumber);
                default:
                    throw new Exception("Unknown figure type");
            }
        }

        public abstract bool IsMoveable { get; }

        public abstract bool IsMoveValid(KaNoBuMoveResponseModel.MoveStep moveStep);

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

        public IFigure CopyForPlayer(int playerId)
        {
            if (this.PlayerId == playerId || playerId == -1 || this.VisibleForAllPlayers)
            {
                return Create(this.PlayerId, this.FigureType, this.VisibleForAllPlayers, this.WinNumber);
            }

            return Create(this.PlayerId, FigureTypes.Unknown, this.VisibleForAllPlayers, this.WinNumber);
        }

        public override string ToString()
        {
            return this.PlayerId + this.FigureType.PrintableName();
        }
    }
}
