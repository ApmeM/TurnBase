using Godot;
using System;

public class DragControl : Node2D
{
    public Line2D dragIndicator;

    [Signal]
    public delegate void DragFinished(Vector2 from, Vector2 to);

    public override void _Ready()
    {
        this.dragIndicator = GetNode<Line2D>("DragIndicator");
        this.dragIndicator.Visible = false;
        this.dragIndicator.Points = new []{Vector2.Zero, Vector2.Zero};
    }

    public override void _Process(float delta)
    {
        if (this.dragIndicator.Visible)
        {
            this.dragIndicator.Points = new []{Vector2.Zero, dragIndicator.GetLocalMousePosition()};

            if (Input.IsActionJustReleased("left_click"))
            {
                dragIndicator.Visible = false;
                this.EmitSignal(nameof(DragFinished), this.dragIndicator.ToGlobal(this.dragIndicator.Points[0]), this.dragIndicator.ToGlobal(this.dragIndicator.Points[1]));
            }
        }
    }

    public void StartDragging()
    {
        this.dragIndicator.GlobalPosition = this.GetGlobalMousePosition();
        this.dragIndicator.Points = new []{Vector2.Zero, Vector2.Zero};
        this.dragIndicator.Visible = true;
    }
}
