using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rules = new KaNoBuRules(8, 8);
        // var player = new PlayerConsole();
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules);
        // game.AddPlayer(player);
        game.AddPlayer(new KaNoBuPlayerEasy());
        game.AddPlayer(new KaNoBuPlayerEasy());

        var listener = new PlayerConsoleListener();

        // GameEventListenerConnector.Connect(game, player);
        GameEventListenerConnector.Connect(game, listener);
        
        await game.Play();
    }
}