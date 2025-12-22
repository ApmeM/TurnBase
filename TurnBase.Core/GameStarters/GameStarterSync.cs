namespace TurnBase.Core;

public class GameStarterSync : IGameStarter
{
    private readonly IGame game;

    public GameStarterSync(IGame game)
    {
        this.game = game;
    }

    public async Task play()
    {
        var players = this.game.getPlayers();

        for (int i = 0; i < players.Count(); i++) {
            await this.game.initPlayer(i);
        }

        await this.game.gameProcess();
    }
}