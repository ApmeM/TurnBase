namespace TurnBase.KaNoBu
{
    public sealed class ShipMineKaNoBuFigure : KaNoBuFigure
    {
        public ShipMineKaNoBuFigure(int playerId, int winNumber)
            : base(playerId, winNumber)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipMine;

        public override bool IsMoveable => false;

        public override Point[] GetPossibleMoveOffsets()
        {
            return new Point[0];
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
