using Godot;

[Tool]
[SceneReference("LevelMapUnit.tscn")]
public partial class LevelMapUnit
{
    [Export]
    public PackedScene GameToLaunch;

    private string levelDescription;
    [Export]
    public string LevelDescription
    {
        get => this.levelDescription;
        set
        {
            this.levelDescription = value;
            if (IsInsideTree())
            {
                this.description.Text = value;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.AddToGroup(Groups.LevelButton);

        this.LevelDescription = this.levelDescription;
    }
}
