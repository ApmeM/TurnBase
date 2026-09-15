using Godot;

[Tool]
[SceneReference("LevelMapUnit.tscn")]
public partial class LevelMapUnit
{
    [Export]
    public PackedScene GameToLaunch;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.AddToGroup(Groups.LevelButton);
    }
}
