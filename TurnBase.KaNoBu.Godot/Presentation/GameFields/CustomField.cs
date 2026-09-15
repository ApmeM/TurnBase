using Godot;
using TurnBase;

[SceneReference("CustomField.tscn")]
public partial class CustomField
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
    public override void GameLogCurrentField(IField field)
    {
        base.GameLogCurrentField(field);
        
        var mainField = (Field2D)field;
        var needUpdateBitmask = false;
        for (var x = 0; x < mainField.Width; x++)
        {
            for (var y = 0; y < mainField.Height; y++)
            {
                var pos = new Vector2(x, y);
                if (this.field.GetCellv(pos) == 4 && mainField.walls[x, y])
                {
                    this.field.SetCellv(pos, -1);
                }
                if (this.field.GetCellv(pos) == -1 && !mainField.walls[x, y])
                {
                    this.field.SetCellv(pos, 4);
                    this.beach.SetCellv(pos, -1);
                    this.castle.SetCellv(pos, -1);
                    this.castle.SetCellv(pos + Vector2.Down, -1);
                    this.castle.SetCellv(pos + Vector2.Up, -1);
                    this.castle.SetCellv(pos + Vector2.Left, -1);
                    this.castle.SetCellv(pos + Vector2.Right, -1);
                    needUpdateBitmask = true;
                }
            }
        }
        if (needUpdateBitmask)
        {
            this.beach.UpdateBitmaskRegion();
            this.castle.UpdateBitmaskRegion();
        }
    }
}
