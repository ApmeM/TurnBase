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
    }

    public virtual IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> Start()
    {
        var rules = new KaNoBuLevelRules(8, true);
        rules.SetInitialField(this.generateField());
        var game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules, "level");
        game.AddPlayer(this);
        game.AddPlayer(new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>());
        return game;
    }

    public Field2D generateField()
    {
        var field2D = Field2D.Create(8, 8);
        var allUnits = this.field.GetChildren();
        foreach (Unit unit in allUnits)
        {
            var fig = new KaNoBuFigure(unit.PlayerNumber, unit.UnitType, true, 0);
            var pos = this.WorldToMap(unit.Position);
            var x = (int)pos.x;
            var y = (int)pos.y;
            field2D[x, y] = fig;
        }
        for (var x = 0; x < 8; x++)
            for (var y = 0; y < 8; y++)
            {
                var pos = new Vector2(x, y);
                field2D.walls[x, y] = this.beach.GetCellv(pos) >= 0;
            }
        GD.Print(field2D);
        return field2D;
    }
}
