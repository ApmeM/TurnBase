using Godot;
using System.Linq;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("Level1.tscn")]
public partial class Level1
{
    public sealed class ShipReachesColumn : IVictoryCondition
    {
        private readonly int goalColumn;

        public ShipReachesColumn(int goalColumn)
        {
            this.goalColumn = goalColumn;
        }

        public bool IsWinner(KaNoBuField mainField, KaNoBuRules rules, int playerNumber)
        {
            if (this.goalColumn < 0 || this.goalColumn >= mainField.Field.Width)
            {
                return false;
            }

            for (var y = 0; y < mainField.Field.Height; y++)
            {
                var figure = mainField.Field.GetFigure(this.goalColumn, y) as KaNoBuFigure;
                if (figure != null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    protected override KaNoBuLevelRules CreateRules()
    {
        var victoryCondition = new ShipReachesColumn(6);
        var looseCondition = new NoMoveableFiguresLooseCondition();
        
        var gameOverCondition = new OrGameOverCondition(
            new PlayerVictoryGameOverCondition(0, victoryCondition),
            new PlayerLooseGameOverCondition(0, looseCondition));

        return new KaNoBuLevelRules(
            8,
            true,
            victoryCondition,
            gameOverCondition,
            looseCondition
            );
    }
}
