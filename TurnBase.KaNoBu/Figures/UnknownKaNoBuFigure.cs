namespace TurnBase.KaNoBu
{
    public sealed class UnknownKaNoBuFigure : KaNoBuFigure
    {
        public UnknownKaNoBuFigure(int playerId, int winNumber)
            : base(playerId, winNumber)
        {
        }

        public override FigureTypes FigureType => FigureTypes.Unknown;

        public override bool IsMoveable => false;

        public override Point[] GetPossibleMoveOffsets()
        {
            return new Point[0];
        }

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            throw new System.Exception("Can not resolve battle with unknown ship");
        }
    }
}
