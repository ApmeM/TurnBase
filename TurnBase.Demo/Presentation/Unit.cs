using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TurnBase.KaNoBu;

[SceneReference("Unit.tscn")]
public partial class Unit
{
    public Queue<Func<Task>> PendingTasks = new Queue<Func<Task>>();
    public Task CurrentTask;

    private KaNoBuFigure.FigureTypes unitType = KaNoBuFigure.FigureTypes.ShipPaper;
    private int playerNumber = 0;
    private bool isSelected = false;
    public Vector2? TargetPositionMap;

    [Signal]
    public delegate void UnitClicked();

    public KaNoBuFigure.FigureTypes UnitType
    {
        get => this.unitType;
        set
        {
            this.unitType = value;
            if (IsInsideTree())
            {
                if (TargetPositionMap == null)
                {
                    return;
                }

                var shipTypeTexture = (AtlasTexture)this.shipTypeFlag.Texture;
                switch (this.unitType)
                {
                    case KaNoBuFigure.FigureTypes.Unknown:
                        shipTypeTexture.Region = new Rect2(280, 170, 20, 20);
                        break;
                    case KaNoBuFigure.FigureTypes.ShipStone:
                        shipTypeTexture.Region = new Rect2(300, 170, 20, 20);
                        break;
                    case KaNoBuFigure.FigureTypes.ShipScissors:
                        shipTypeTexture.Region = new Rect2(280, 190, 20, 20);
                        break;
                    case KaNoBuFigure.FigureTypes.ShipPaper:
                        shipTypeTexture.Region = new Rect2(300, 190, 20, 20);
                        break;
                    case KaNoBuFigure.FigureTypes.ShipFlag:
                        shipTypeTexture.Region = new Rect2(320, 170, 20, 20);
                        break;
                }
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
                if (this.TargetPositionMap == null)
                {
                    return;
                }

                var shipTexture = (AtlasTexture)this.ship.Texture;
                shipTexture.Region = new Rect2(0, 120 * this.playerNumber, 66, 113);
            }
        }
    }

    [Export]
    public bool IsSelected
    {
        get => this.isSelected; set
        {
            this.isSelected = value;
            if (IsInsideTree())
            {
                this.flag.Visible = this.IsSelected;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.IsSelected = this.isSelected;
        this.PlayerNumber = this.playerNumber;
        this.UnitType = this.unitType;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (CurrentTask != null && CurrentTask.IsCompleted)
        {
            CurrentTask = null;
        }

        if (CurrentTask == null && PendingTasks.Count > 0)
        {
            CurrentTask = PendingTasks.Dequeue().Invoke();
        }

        if (Input.IsActionJustPressed("left_click"))
        {
            if (this.ship.GetRect().HasPoint(ship.GetLocalMousePosition()))
            {
                EmitSignal(nameof(UnitClicked));
            }
        }
    }

    public void UnitHit()
    {
        this.TargetPositionMap = null;
        this.PendingTasks.Enqueue(() => UnitHitAction());
    }

    public async Task UnitHitAction()
    {
        var texture = (AtlasTexture)this.ship.Texture;

        texture.Region = new Rect2(70, texture.Region.Position.y, texture.Region.Size);
        await this.ToSignal(this.GetTree().CreateTimer(0.3f), "timeout");
        texture.Region = new Rect2(140, texture.Region.Position.y, texture.Region.Size);
        await this.ToSignal(this.GetTree().CreateTimer(0.3f), "timeout");
        texture.Region = new Rect2(210, texture.Region.Position.y, texture.Region.Size);
        this.GetParent().MoveChild(this, 1);
        this.waveGenerator.Stop();
    }

    public void Attack()
    {
        this.PendingTasks.Enqueue(() => AttackAction());
        var star = (TextureRect)this.starExample.Duplicate();
        star.Visible = true;
        this.stars.AddChild(star);
    }

    public async Task AttackAction()
    {
        const float MOVE_SPEED = 200;

        const float LIFETIME = 0.5f;

        var cannonBall = (Sprite)this.cannonBall.Duplicate();
        cannonBall.Visible = true;
        this.AddChild(cannonBall);

        var tween = new Tween();
        this.AddChild(tween);
        tween.InterpolateProperty(cannonBall, "position", Vector2.Zero, Vector2.Down * MOVE_SPEED * LIFETIME, LIFETIME);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
        tween.QueueFree();

        cannonBall.QueueFree();
    }

    public void MoveUnitTo(Vector2 newCell, Vector2 newPosition)
    {
        this.TargetPositionMap = newCell;
        this.PendingTasks.Enqueue(() => MoveUnitToAction(newPosition));
        this.waveGenerator.Start();
    }

    public async Task MoveUnitToAction(Vector2 newPosition)
    {
        const float MOVE_SPEED = 160;

        var tween = new Tween();
        this.AddChild(tween);
        tween.InterpolateProperty(this, "position", this.Position, newPosition, (this.Position - newPosition).Length() / MOVE_SPEED);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
        tween.QueueFree();
    }

    public void RotateUnitTo(Vector2 lookAtPosition)
    {
        this.PendingTasks.Enqueue(() => RotateUnitToAction(lookAtPosition));
    }

    public async Task RotateUnitToAction(Vector2 lookAtPosition)
    {
        var newRotation = (lookAtPosition - this.Position).Angle() - Mathf.Pi / 2;
        var rotationDistance = Math.Abs(newRotation - this.Rotation);

        if (Math.Abs(newRotation + Mathf.Pi * 2 - this.Rotation) < rotationDistance)
        {
            newRotation = newRotation + Mathf.Pi * 2;
            rotationDistance = Math.Abs(newRotation + Mathf.Pi * 2 - this.Rotation);
        }
        else if (Math.Abs(newRotation - this.Rotation) < rotationDistance)
        {
            newRotation = newRotation - Mathf.Pi * 2;
            rotationDistance = Math.Abs(newRotation - this.Rotation);
        }

        const float ROTATION_SPEED = 20f;

        var tween = new Tween();
        this.AddChild(tween);
        tween.InterpolateProperty(this, "rotation", this.Rotation, newRotation, rotationDistance / ROTATION_SPEED);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
        tween.QueueFree();
    }

    public void CallbackForUnit(Func<Unit, Task> callback)
    {
        this.PendingTasks.Enqueue(() => callback(this));
    }

    public void CancelActions()
    {
        this.PendingTasks.Clear();
    }
}
