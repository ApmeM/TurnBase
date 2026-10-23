namespace TurnBase.KaNoBu
{
    public sealed class ShipDreadnoughtKaNoBuFigure : KaNoBuFigure
    {
        public ShipDreadnoughtKaNoBuFigure(int playerId, int winNumber)
            : base(playerId, winNumber)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipDreadnought;

        public override bool IsMoveable => true;

        public override Point[] GetPossibleMoveOffsets()
        {
            return new[]
            {
                new Point(0, 1),
                new Point(0, -1),
                new Point(1, 0),
                new Point(-1, 0),
            };
        }

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            switch (defender.FigureType)
            {
                case FigureTypes.Unknown:
                    throw new System.Exception("Can not resolve battle with unknown ship");
                case FigureTypes.ShipMine:
                    return BattleResolution.BothAreDestroyed();
                case FigureTypes.ShipFlag:
                case FigureTypes.ShipStone:
                case FigureTypes.ShipPaper:
                case FigureTypes.ShipScissors:
                case FigureTypes.ShipUniversal:
                case FigureTypes.ShipScout:
                case FigureTypes.ShipDreadnought:
                    return BattleResolution.AttackerWon(this);
                default:
                    throw new System.Exception($"Unsupported figure type {defender.FigureType}");
            }
        }
    }
}