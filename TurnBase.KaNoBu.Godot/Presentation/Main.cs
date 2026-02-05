using Godot;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("Main.tscn")]
public partial class Main
{
    public override void _Ready()
    {
        this.FillMembers();

        this.uI.Connect(nameof(UI.StartGameEventhandler), this, nameof(OnStartGameAsync));
        this.infinityGameField.RemoveFromGroup(Groups.Field);

        StartInfinityGame();
    }

    private async void StartInfinityGame()
    {
        while (true)
        {
            var rules = new KaNoBuRules(8);
            rules.AllFiguresVisible = true;
            var kanobu = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules, "test");
            for (var i = 0; i < 4; i++)
            {
                var playerEasy = new KaNoBuPlayerEasy();
                var delayedPlayer = new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(
                    playerEasy,
                    async (delay) => await this.ToSignal(this.GetTree().CreateTimer(delay / 1000f), "timeout"),
                    1,
                    300);
                kanobu.AddPlayer(delayedPlayer);
            }
            kanobu.AddGameLogListener(this.infinityGameField);
            await kanobu.Play();
        }
    }

    private async void OnStartGameAsync()
    {
        var game = this.uI.BuildGame();
        if (game == null)
        {
            return;
        }

        this.uI.Visible = false;
        this.infinityGameField.Visible = false;

        await game.Play();

        this.GetTree().CallGroup(Groups.Field, "queue_free");

        this.infinityGameField.Visible = true;
        this.uI.Visible = true;
    }
}
