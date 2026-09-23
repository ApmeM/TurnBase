namespace TurnBase.KaNoBu
{
    public sealed class ShipFlagKaNoBuFigure : KaNoBuFigure
    {
        public ShipFlagKaNoBuFigure(int playerId, int winNumber)
            : base(playerId, winNumber)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipFlag;

        public override bool IsMoveable => false;

        public override Point[] GetPossibleMoveOffsets()
        {
            return new Point[0];
        }

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            switch (defender.FigureType)
            {
                case FigureTypes.Unknown:
                    throw new System.Exception("Can not resolve battle with unknown ship");
                case FigureTypes.ShipStone:
                case FigureTypes.ShipPaper:
                case FigureTypes.ShipScissors:
                case FigureTypes.ShipUniversal:
                case FigureTypes.ShipMine:
                case FigureTypes.ShipScout:
                case FigureTypes.ShipFlag:
                    throw new System.Exception("Flag can not initialize battle");
                default:
                    throw new System.Exception($"Unsupported figure type {defender.FigureType}");
            }
        }
    }
}
