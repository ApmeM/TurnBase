using System;
using System.Linq;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("LevelBase.tscn")]
public partial class LevelBase
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.AddToGroup(Groups.Level);

        this.beach.GetUsedCells()
            .Cast<Vector2>()
            .Where(point => this.castle.GetCellv(point) == -1)
            .ToList()
            .ForEach(point => this.castle.SetCellv(point, 6));
        this.castle.UpdateBitmaskRegion();
    }

    public void Initialize()
    {
        var rules = new KaNoBuLevelRules(8, true);
        rules.SetInitialField(this.generateField());
        this.Game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>(rules, "level");
        this.Game.AddPlayer(this);
        this.Game.AddPlayer(new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>());
    }

    public Field2D generateField()
    {
        var cells = this.field.GetUsedCells().Cast<Vector2>().ToList();

        var left = (int)cells.Min(a => a.x);
        var right = (int)cells.Max(a => a.x + 1);
        var top = (int)cells.Min(a => a.y);
        var bottom = (int)cells.Max(a => a.y + 1);

        if (left < 0 || top < 0)
        {
            throw new Exception("Unsupported");
        }

        var field2D = Field2D.Create(right, bottom);
        var allUnits = this.field.GetChildren();
        foreach (Unit unit in allUnits)
        {
            var fig = KaNoBuFigure.Create(unit.PlayerNumber, unit.UnitType, 0);
            var pos = this.field.WorldToMap(unit.Position);
            var x = (int)pos.x;
            var y = (int)pos.y;
            field2D[x, y] = fig;
        }
        for (var x = 0; x < right; x++)
            for (var y = 0; y < bottom; y++)
            {
                var pos = new Vector2(x, y);
                field2D.walls[x, y] = this.field.GetCellv(pos) < 0;
            }
        return field2D;
    }
}
