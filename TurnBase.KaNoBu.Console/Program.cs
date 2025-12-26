using TurnBase.Core;

namespace TurnBase.KaNoBu;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rules = new KaNoBuRules(8, 8);
        var player = new KaNoBuPlayerConsole();
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules);
        game.AddPlayer(new KaNoBuPlayerEasy());
        game.AddPlayer(player);

        // var listener = new KaNoBuListenerConsole();

        GameEventListenerConnector.Connect(game, player);
        // GameEventListenerConnector.Connect(game, listener);
        
        await game.Play();
    }
}