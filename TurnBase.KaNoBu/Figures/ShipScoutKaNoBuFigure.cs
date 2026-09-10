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

        public override bool IsMoveValid(KaNoBuMoveResponseModel.MoveStep moveStep)
        {
            var distance = System.Math.Abs(moveStep.From.X - moveStep.To.X) +
                System.Math.Abs(moveStep.From.Y - moveStep.To.Y);
            return distance > 0 && distance <= 2;
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