using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rules = new KaNoBuRules(8, 8);
        var player = new PlayerConsole();
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules);
        game.AddPlayer(player);
        game.AddPlayer(new KaNoBuPlayerEasy());

        game.GameStarted += player.gameStarted;
        game.GamePlayerInitialized += player.playerInitialized;
        game.GamePlayerWrongTurn += player.playerWrongTurnMade;
        game.GamePlayerTurn += player.playerTurnMade;
        game.GameFinished += player.gameFinished;
        
        await game.Play(true);
    }
}