using TurnBase.KaNoBu;

namespace TurnBase
{
    public class NoMoveableFiguresLooseCondition : ILooseCondition
    {
        public bool IsLooser(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            for (var x = 0; x < mainField.Field.Width; x++)
            {
                for (var y = 0; y < mainField.Field.Height; y++)
                {
                    var figure = mainField.Field.GetFigure(x, y) as KaNoBuFigure;
                    if (figure != null && figure.PlayerId == playerNumber && figure.IsMoveable)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}