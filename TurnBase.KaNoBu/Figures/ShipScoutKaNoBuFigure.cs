namespace TurnBase.KaNoBu
{
    public sealed class ShipScoutKaNoBuFigure : KaNoBuFigure
    {
        public ShipScoutKaNoBuFigure(int playerId, int winNumber)
            : base(playerId, winNumber)
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

        public override bool IsMoveValid(KaNoBuMoveResponseModel.MoveStep moveStep, Field2D mainField)
        {
            if (!base.IsMoveValid(moveStep, mainField))
            {
                return false;
            }

            var offsetX = moveStep.To.X - moveStep.From.X;
            var offsetY = moveStep.To.Y - moveStep.From.Y;
            if ((System.Math.Abs(offsetX) == 2 && offsetY == 0) ||
                (System.Math.Abs(offsetY) == 2 && offsetX == 0))
            {
                var middle = new Point(
                    moveStep.From.X + offsetX / 2,
                    moveStep.From.Y + offsetY / 2);
                var middleFigure = (KaNoBuFigure)mainField.GetFigure(middle);
                return middleFigure == null || middleFigure.PlayerId == this.PlayerId;
            }

            return true;
        }

        public override BattleResolution ResolveBattle(KaNoBuFigure defender)
        {
            switch (defender.FigureType)
            {
                case FigureTypes.Unknown:
                    throw new System.Exception("Can not resolve battle with unknown ship");
                case FigureTypes.ShipFlag:
                    return BattleResolution.FlagCaptured(this);
                case FigureTypes.ShipStone:
                case FigureTypes.ShipPaper:
                case FigureTypes.ShipScissors:
                case FigureTypes.ShipMine:
                case FigureTypes.ShipScout:
                case FigureTypes.ShipUniversal:
                case FigureTypes.ShipDreadnought:
                    return BattleResolution.Scouted();
                default:
                    throw new System.Exception($"Unsupported figure type {defender.FigureType}");
            }
        }
    }
}