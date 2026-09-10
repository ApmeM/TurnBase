namespace TurnBase.KaNoBu
{
    public sealed class ShipPaperKaNoBuFigure : KaNoBuFigure
    {
        public ShipPaperKaNoBuFigure(int playerId, bool visibleForAllPlayers, int winNumber)
            : base(playerId, visibleForAllPlayers, winNumber)
        {
        }

        public override FigureTypes FigureType => FigureTypes.ShipPaper;

        public override bool IsMoveable => true;

        public override bool IsMoveValid(KaNoBuMoveResponseModel.MoveStep moveStep)
        {
            return moveStep.From.IsAdjacentTo(moveStep.To);
        }

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            switch (defender.FigureType)
            {
                case FigureTypes.Unknown:
                    throw new System.Exception("Can not resolve battle with unknown ship");
                case FigureTypes.ShipPaper:
                    return BattleResolution.Draw();
                case FigureTypes.ShipFlag:
                case FigureTypes.ShipStone:
                case FigureTypes.ShipScout:
                    return BattleResolution.AttackerWon(this);
                case FigureTypes.ShipScissors:
                    return BattleResolution.DefenderWon(defender);
                case FigureTypes.ShipMine:
                    return BattleResolution.BothAreDestroyed();
                case FigureTypes.ShipUniversal:
                    return BattleResolution.DefenderWon(defender.WithFigureType(FigureTypes.ShipScissors));
                default:
                    throw new System.Exception($"Unsupported figure type {defender.FigureType}");
            }
        }
    }
}
