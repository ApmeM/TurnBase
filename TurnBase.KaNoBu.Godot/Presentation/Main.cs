using System.Threading.Tasks;
using Godot;

[SceneReference("Main.tscn")]
public partial class Main
{
    public override void _Ready()
    {
        this.FillMembers();

        this.uI.Connect(nameof(UI.StartGameEventhandler), this, nameof(OnStartGameAsync));
    }

    private async void OnStartGameAsync()
    {
        var game = this.uI.BuildGame(this.gameField);
        if (game == null)
        {
            return;
        }

        this.uI.Visible = false;

        await game.Play();

        this.uI.Visible = true;
    }
}
