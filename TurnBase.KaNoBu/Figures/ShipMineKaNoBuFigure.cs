namespace TurnBase.KaNoBu
{
    public sealed class ShipMineKaNoBuFigure : KaNoBuFigure
    {
        public ShipMineKaNoBuFigure(int playerId, bool visibleForAllPlayers, int winNumber)
            : base(playerId, visibleForAllPlayers, winNumber)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipMine;

        public override bool IsMoveable => false;

        public override bool IsMoveValid(KaNoBuMoveResponseModel.MoveStep moveStep)
        {
            return false;
        }

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            switch (defender.FigureType)
            {
                case FigureTypes.ShipMine:
                case FigureTypes.ShipFlag:
                case FigureTypes.ShipStone:
                case FigureTypes.ShipPaper:
                case FigureTypes.ShipScissors:
                case FigureTypes.ShipUniversal:
                case FigureTypes.ShipScout:
                    throw new System.Exception("Mine can not initialize battle");
                case FigureTypes.Unknown:
                    throw new System.Exception("Can not resolve battle with unknown ship");
                default:
                    throw new System.Exception($"Unsupported figure type {defender.FigureType}");
            }
        }
    }
}
