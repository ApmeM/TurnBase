using TurnBase;

namespace TurnBase.KaNoBu;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rules = new KaNoBuRules(8);
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>(rules, "test");
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>(new KaNoBuPlayerEasy(), async (delay) => await Task.Delay(delay), 1, 500));
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>(new KaNoBuPlayerEasy(), async (delay) => await Task.Delay(delay), 1, 500));
        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>(new KaNoBuPlayerEasy(), async (delay) => await Task.Delay(delay), 1, 500));

        game.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>(new KaNoBuPlayerEasy(), async (delay) => await Task.Delay(delay), 1, 500));

        // game.AddPlayer(new KaNoBuPlayerConsole());
        game.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel, Field2D>(new ConsoleLogger()));

        await game.Play();
    }
}