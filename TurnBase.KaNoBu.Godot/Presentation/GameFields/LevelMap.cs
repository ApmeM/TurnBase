using Godot;

[SceneReference("LevelMap.tscn")]
public partial class LevelMap
{
    [Signal]
    public delegate void LevelSelected(LevelMapUnit mapUnit);

    public override async void _Ready()
    {
        this.FillMembers();

        foreach (var mapUnit in this.GetTree().GetNodesInGroup(Groups.LevelButton))
        {
            if (mapUnit is LevelMapUnit levelMapUnit)
            {
                levelMapUnit.Connect(nameof(Unit.UnitClicked), this, nameof(OnLevelPressed), new Godot.Collections.Array { levelMapUnit });
            }
        }

        this.highlitePointer.Show(new Vector2(-1, -1), 1f);
        await this.dialog.Show("Hello captain! Nice to see you here again. Those enemies are attacking us and we need to defeat them to protect our land!", true, null);
        this.highlitePointer.Show(new Vector2(270, 654), 50f);
        await this.dialog.Show("Hello! I'll do my best. Click on this ship to attack them.", false, null);
        await this.dialog.Show("To arms my brothers!", true, null);
    }

    public override async void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("left_click") && @event is InputEventMouseButton mouseEvent)
        {
            this.GetTree().SetInputAsHandled();
            this.player.CancelAnimations();
            this.player.CallbackAnimation((unit) => unit.RotateUnitToAnimation(mouseEvent.Position));
            this.player.CallbackAnimation((unit) => unit.MoveUnitToAnimation(mouseEvent.Position));
        }
    }

    public async void OnLevelPressed(LevelMapUnit mapUnit)
    {
        this.player.CancelAnimations();
        this.player.CallbackAnimation((unit) => unit.RotateUnitToAnimation(mapUnit.Position));
        this.player.CallbackAnimation((unit) => unit.MoveUnitToAnimation(mapUnit.Position - (mapUnit.Position - this.player.Position).Normalized() * 64));
        this.player.CallbackAnimation((unit) => mapUnit.RotateUnitToAnimation(this.player.Position));
        this.player.CallbackAnimation(async (unit) => this.EmitSignal(nameof(LevelSelected), mapUnit));
    }

    public async void LevelFinished(LevelMapUnit mapUnit, bool result)
    {
        if (result)
        {
            mapUnit.IsClickable = false;
            await this.player.AttackAnimation();
            await mapUnit.UnitHitAnimation();
        }
        else
        {
            var scene = new PackedScene();

            var packResult = scene.Pack(this.player);
            if (packResult != Error.Ok)
            {
                GD.PrintErr($"Не удалось запаковать Node: {packResult}");
                return;
            }

            var newPlayer = scene.Instance<Unit>();;
            this.field.AddChild(newPlayer);
            this.player.Visible = false;
            await newPlayer.UnitHitAnimation();
            
            this.player.Position = new Vector2(85, 710);
            await this.player.RotateUnitToAnimation(new Vector2(85, 670));
            this.player.Visible = true;
            await this.player.MoveUnitToAnimation(new Vector2(85, 670));
        }
    }
}
