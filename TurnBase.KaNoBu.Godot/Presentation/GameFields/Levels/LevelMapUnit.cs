using Godot;
using System.Threading.Tasks;
using TurnBase.KaNoBu;

[Tool]
[SceneReference("LevelMapUnit.tscn")]
public partial class LevelMapUnit
{
    private bool isClickable;
    private KaNoBuFigure.FigureTypes unitType = KaNoBuFigure.FigureTypes.ShipPaper;
    private int playerNumber;
    private string levelDescription;

    [Signal]
    public delegate void UnitClicked();

    [Export]
    public bool IsClickable
    {
        get => this.isClickable;
        set
        {
            this.isClickable = value;
            if (IsInsideTree())
            {
                this.unit.IsClickable = value;
            }
        }
    }

    [Export]
    public KaNoBuFigure.FigureTypes UnitType
    {
        get => this.unitType;
        set
        {
            this.unitType = value;
            if (IsInsideTree())
            {
                this.unit.UnitType = value;
            }
        }
    }

    [Export]
    public int PlayerNumber
    {
        get => this.playerNumber;
        set
        {
            this.playerNumber = value;
            if (IsInsideTree())
            {
                this.unit.PlayerNumber = value;
            }
        }
    }

    [Export]
    public PackedScene GameToLaunch;

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

        this.unit.Connect(nameof(Unit.UnitClicked), this, nameof(OnUnitClicked));
        this.unit.IsClickable = this.isClickable;
        this.unit.UnitType = this.unitType;
        this.unit.PlayerNumber = this.playerNumber;
        this.LevelDescription = this.levelDescription;
    }

    private void OnUnitClicked()
    {
        this.EmitSignal(nameof(UnitClicked));
    }

    public Task RotateUnitToAnimation(Vector2 lookAtPosition)
    {
        var targetInUnitParent = this.ToLocal(this.GetParent<Node2D>().ToGlobal(lookAtPosition));
        return this.unit.RotateUnitToAnimation(targetInUnitParent);
    }

    public async Task UnitHitAnimation()
    {
        await this.unit.UnitHitAnimation();
        this.unit.ZIndex = 0;
        this.ZIndex = -1;
        this.GetParent().MoveChild(this, 1);
    }
}
