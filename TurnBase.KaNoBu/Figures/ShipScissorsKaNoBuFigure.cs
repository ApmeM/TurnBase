namespace TurnBase.KaNoBu
{
    public sealed class ShipScissorsKaNoBuFigure : KaNoBuFigure
    {
        public ShipScissorsKaNoBuFigure(int playerId, int winNumber)
            : base(playerId, winNumber)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipScissors;

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
                case FigureTypes.ShipScissors:
                    return BattleResolution.Draw();
                case FigureTypes.ShipFlag:
                case FigureTypes.ShipPaper:
                case FigureTypes.ShipScout:
                    return BattleResolution.AttackerWon(this);
                case FigureTypes.ShipStone:
                    return BattleResolution.DefenderWon(defender);
                case FigureTypes.ShipMine:
                    return BattleResolution.BothAreDestroyed();
                case FigureTypes.ShipUniversal:
                    return BattleResolution.DefenderWon(defender.WithFigureType(FigureTypes.ShipStone));
                default:
                    throw new System.Exception($"Unsupported figure type {defender.FigureType}");
            }
        }
    }
}
