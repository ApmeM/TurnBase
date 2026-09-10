namespace TurnBase.KaNoBu
{
    public sealed class ShipScoutKaNoBuFigure : KaNoBuFigure
    {
        public ShipScoutKaNoBuFigure(int playerId, bool visibleForAllPlayers, int winNumber)
            : base(playerId, visibleForAllPlayers, winNumber)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipScout;

        public override bool IsMoveable => true;

        public override Point[] GetPossibleMoveOffsets()
        {
            return new[]
            {
                new Point(0, 1),
                new Point(0, -1),
                new Point(1, 0),
                new Point(-1, 0),
                new Point(0, 2),
                new Point(0, -2),
                new Point(2, 0),
                new Point(-2, 0),
                new Point(1, 1),
                new Point(1, -1),
                new Point(-1, 1),
                new Point(-1, -1),
            };
        }

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            switch (defender.FigureType)
            {
                case FigureTypes.Unknown:
                    throw new System.Exception("Can not resolve battle with unknown ship");
                case FigureTypes.ShipFlag:
                    return BattleResolution.AttackerWon(this);
                case FigureTypes.ShipStone:
                case FigureTypes.ShipPaper:
                case FigureTypes.ShipScissors:
                    return BattleResolution.DefenderWon(defender);
                case FigureTypes.ShipMine:
                    return BattleResolution.BothAreDestroyed();
                case FigureTypes.ShipScout:
                    return BattleResolution.Draw();
                case FigureTypes.ShipUniversal:
                    return BattleResolution.DefenderWon(defender.WithFigureType(FigureTypes.ShipScout));
                default:
                    throw new System.Exception($"Unsupported figure type {defender.FigureType}");
            }
        }
    }
}