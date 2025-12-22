using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rules = new GameRulesKaNoBu(8, 8);
        var player = new PlayerConsole();
        var game = new Game(rules);
        game.addPlayer(new PlayerAIEasy());
        game.addPlayer(player);

        game.GameStarted += player.gameStarted;
        game.GamePlayerInitialized += player.playerInitialized;
        game.GamePlayerWrongTurn += player.playerWrongTurnMade;
        game.GamePlayerTurn += player.playerTurnMade;
        game.GameFinished += player.gameFinished;
        
        var starter = new GameStarterSync(game);
        await starter.play();
    }
}