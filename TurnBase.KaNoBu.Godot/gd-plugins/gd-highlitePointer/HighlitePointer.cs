using Godot;

/// <summary>
/// Full-screen tutorial overlay. Add the HighlitePointer scene to the UI and await Show.
/// The position is in viewport (screen) coordinates.
/// </summary>
[Tool]
[SceneReference("HighlitePointer.tscn")]
public partial class HighlitePointer
{
    [Signal]
    public delegate void ClickedInsideCircle();

    [Export]
    public Color BackgroundColor
    {
        get => backgroundColor;
        set
        {
            backgroundColor = value;
            if (this.IsInsideTree())
            {
                ((ShaderMaterial)this.shade.Material).SetShaderParam("shade_color", value);
            }
        }
    }

    private Color backgroundColor = new Color(0, 0, 0, 0.427451f);

    [Export]
    public Vector2 Center
    {
        get => center;
        set
        {
            center = value;
            if (this.IsInsideTree())
            {
                ((ShaderMaterial)this.shade.Material).SetShaderParam("hole_center", value);
                this.fingerAnimation.RectPosition = this.Center - this.finger.RectSize - Vector2.One.Normalized() * this.Radius / 2;
            }
        }
    }

    private Vector2 center = new Vector2(270, 654);

    [Export]
    public float Radius
    {
        get => radius;
        set
        {
            if (value <= 0)
            {
                GD.PrintErr("HighlitePointer: Radius must be greater than 0.");
                return;
            }

            radius = value;
            if (this.IsInsideTree())
            {
                ((ShaderMaterial)this.shade.Material).SetShaderParam("hole_radius", value);
                this.fingerAnimation.RectPosition = this.Center - this.finger.RectSize - Vector2.One.Normalized() * this.Radius / 2;
            }
        }
    }

    private float radius = 50f;

    private Vector2 lastShaderViewportSize = Vector2.Zero;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.BackgroundColor = this.BackgroundColor;
        this.Center = this.Center;
        this.Radius = this.Radius;
        this.UpdateShaderViewportSize();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        this.UpdateShaderViewportSize();
    }

    private void UpdateShaderViewportSize()
    {
        if (this.shade == null)
        {
            GD.PrintErr("HighlitePointer: shade node is null. Make sure the scene is set up correctly.");
            return;
        }

        // Shader UV spans the ColorRect itself, so use its actual size, not the editor's outer viewport size.
        var size = this.shade.RectSize;
        if (size.x <= 0 || size.y <= 0)
        {
            GD.PrintErr("HighlitePointer: shader RectSize is invalid");
            return;
        }

        if (size == this.lastShaderViewportSize)
        {
            return;
        }

        ((ShaderMaterial)this.shade.Material).SetShaderParam("viewport_size", size);
        this.lastShaderViewportSize = size;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (!this.Visible)
        {
            return;
        }

        Vector2 clickPosition;
        if (@event is InputEventMouseButton mouse)
        {
            if (!mouse.Pressed || mouse.ButtonIndex != (int)ButtonList.Left)
            {
                return;
            }

            clickPosition = mouse.Position;
        }
        else if (@event is InputEventScreenTouch touch)
        {
            if (!touch.Pressed)
            {
                return;
            }

            clickPosition = touch.Position;
        }
        else
        {
            return;
        }

        var material = (ShaderMaterial)this.shade.Material;
        var center = (Vector2)material.GetShaderParam("hole_center");
        var radius = (float)material.GetShaderParam("hole_radius");
        if (clickPosition.DistanceTo(center) > radius)
        {
            this.GetTree().SetInputAsHandled();
            return;
        }

        this.EmitSignal(nameof(ClickedInsideCircle));
    }
}
