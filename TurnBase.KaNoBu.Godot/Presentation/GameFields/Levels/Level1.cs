using Godot;
using System.Linq;
using TurnBase;
using TurnBase.KaNoBu;


[SceneReference("Level1.tscn")]
public partial class Level1
{

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    protected override KaNoBuLevelRules CreateRules()
    {
        var victoryCondition = new ShipReachesColumnGameCondtion(6);
        var looseCondition = new NoMoveableFiguresGameCondition();
        var playerZeroGameOverCondition = new AndGameCondition(
            new PlayerFilterGameCondtion(0),
            new OrGameCondition(victoryCondition, looseCondition));
        var gameOverCondition = new AnyPlayerGameOverCondition(playerZeroGameOverCondition);

        return new KaNoBuLevelRules(
            8,
            true,
            victoryCondition,
            gameOverCondition,
            looseCondition
            );
    }
}
