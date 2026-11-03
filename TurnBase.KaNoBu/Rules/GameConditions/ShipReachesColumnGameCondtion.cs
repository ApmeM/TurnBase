using TurnBase;
using TurnBase.KaNoBu;

public class ShipReachesColumnGameCondtion : IGameCondition
{
    private readonly int goalColumn;

    public ShipReachesColumnGameCondtion(int goalColumn)
    {
        this.goalColumn = goalColumn;
    }

    public bool CheckCondition(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
    {
        if (this.goalColumn < 0 || this.goalColumn >= mainField.Field.Width)
        {
            return false;
        }

        for (var y = 0; y < mainField.Field.Height; y++)
        {
            var figure = mainField.Field.GetFigure(this.goalColumn, y) as KaNoBuFigure;
            if (figure != null && figure.PlayerId == playerNumber)
            {
                return true;
            }
        }

        return false;
    }
}
