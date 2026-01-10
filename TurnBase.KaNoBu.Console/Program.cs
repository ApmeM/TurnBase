using TurnBase;

namespace TurnBase.KaNoBu;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rules = new KaNoBuRules(8);
        var console = new KaNoBuPlayerConsole();
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules, "test");
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 500));
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 500));
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 500));
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(new KaNoBuPlayerEasy(), 1, 500));
        // game.AddPlayer(console);
        game.AddGameListener(console);
        game.AddGameLogListener(console);
        
        await game.Play();
    }
}