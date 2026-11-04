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

    public override IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> CreateGame()
    {
        var victoryCondition = new ShipReachesColumnGameCondtion(6);
        var looseCondition = new NoMoveableFiguresGameCondition();
        var playerZeroGameOverCondition = new AndGameCondition(
            new PlayerFilterGameCondtion(0),
            new OrGameCondition(victoryCondition, looseCondition));
        var gameOverCondition = new AnyPlayerGameOverCondition(playerZeroGameOverCondition);

        var rules = new KaNoBuLevelRules(
            8,
            true,
            victoryCondition,
            gameOverCondition,
            looseCondition
            );

        rules.SetInitialField(this.generateField());
        
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>(rules, "level");
        game.AddPlayer(this);
        game.AddPlayer(new KaNoBuPlayerNoMove());
        return game;
    }
}
