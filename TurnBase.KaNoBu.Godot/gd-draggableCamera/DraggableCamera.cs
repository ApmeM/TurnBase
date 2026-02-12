using Godot;
using System;
using System.Linq;

[SceneReference("DraggableCamera.tscn")]
public partial class DraggableCamera
{
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    private bool drag = false;
    private Vector2 initPosMouse = Vector2.Zero;
    private Vector2 initPosCamera = Vector2.Zero;

    [Export]
    public bool IsDebugMode = false;

    [Export]
    public float minimumZoom = 0.3f;

    [Export]
    public float maximumZoom = 3f;

    [Export]
    public bool enabled = true;
    /// <summary>
    ///     The zoom value should be between -1 and 1. 
    ///     This value is then translated to be from minimumZoom to maximumZoom.
    ///     This lets you set appropriate minimum/maximum values then use a more intuitive -1 to 1 mapping to change the zoom.
    /// </summary>
    public float NormalizedZoom
    {
        get
        {
            if (this.Zoom.x == 1)
                return 0f;

            if (this.Zoom.x < 1)
                return Map(this.Zoom.x, this.minimumZoom, 1, -1, 0);
            return Map(this.Zoom.x, 1, this.maximumZoom, 0, 1);
        }
        set
        {
            var newZoom = Mathf.Clamp(value, -1, 1);
            if (newZoom == 0)
                this.Zoom = Vector2.One;
            else if (newZoom < 0)
                this.Zoom = Vector2.One * Map(newZoom, -1, 0, this.minimumZoom, 1);
            else
                this.Zoom = Vector2.One * Map(newZoom, 0, 1, 1, this.maximumZoom);
        }
    }

    /// <summary>
    ///     Minimum non-scaled value (0 - float.Max) that the camera zoom can be. 
    ///     Defaults to 0.3
    /// </summary>
    public float MinimumZoom
    {
        get => this.minimumZoom;
        set
        {
            if (value <= 0)
            {
                throw new Exception("MinimumZoom must be greater then zero.");
            }

            if (this.Zoom.x < value)
                this.Zoom = Vector2.One * value;

            this.minimumZoom = value;
        }
    }

    /// <summary>
    ///     maximum non-scaled value (0 - float.Max) that the camera zoom can be. Defaults to 3
    /// </summary>
    /// <value>The maximum zoom.</value>
    public float MaximumZoom
    {
        get => this.maximumZoom;
        set
        {
            if (value <= 0)
            {
                throw new Exception("MaximumZoom must be greater then zero.");
            }

            if (this.Zoom.x > value)
                this.Zoom = Vector2.One * value;

            this.maximumZoom = value;
        }
    }
    private int dragsCount = 0;
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (!enabled)
        {
            return;
        }

        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (((ButtonList)mouseMotion.ButtonMask & ButtonList.MaskLeft) == ButtonList.Left)
            {
                if (drag)
                {
                    this.GlobalPosition = this.initPosCamera + (this.initPosMouse - this.GetViewport().GetMousePosition()) * this.Zoom;
                    if (IsDebugMode)
                    {
                        this.debugLabel.Text = $"Dragging {dragsCount} at {this.GlobalPosition}";
                    }
                }
                else
                {
                    if (initPosMouse == Vector2.Zero)
                    {
                        this.initPosMouse = this.GetViewport().GetMousePosition();
                        this.initPosCamera = this.GlobalPosition;
                        if (IsDebugMode)
                        {
                            dragsCount++;
                            this.debugLabel.Text = $"Drag {dragsCount} from {this.initPosCamera}";
                        }
                    }

                    this.drag = this.initPosMouse != this.GetViewport().GetMousePosition();
                }
            }
        }

        if (@event is InputEventMouseButton buttonEvent)
        {
            if (buttonEvent.ButtonIndex == (int)ButtonList.Left && buttonEvent.IsReleased())
            {
                if (IsDebugMode)
                {
                    this.debugLabel.Text = $"Drag {dragsCount} stopped";
                }
                this.drag = false;
                this.initPosMouse = Vector2.Zero;
                this.initPosCamera = Vector2.Zero;
            }
            if (buttonEvent.ButtonIndex == (int)ButtonList.WheelUp)
            {
                this.NormalizedZoom -= 0.1f;
            }
            if (buttonEvent.ButtonIndex == (int)ButtonList.WheelDown)
            {
                this.NormalizedZoom += 0.1f;
            }
        }

        if (@event is InputEventMagnifyGesture gesture)
        {
            this.NormalizedZoom -= gesture.Factor - 1;
        }
    }

    private static float Map(float value, float leftMin, float leftMax, float rightMin, float rightMax)
    {
        return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
    }

    public void SetCameraLimits(TileMap floor, Vector2 margins)
    {
        var cells = floor.GetUsedCells().Cast<Vector2>().ToList();
        if(cells.Count == 0)
        {
            return;
        }
        var viewport = this.GetViewport().Size;

        this.LimitLeft = (int)(Math.Min(0, cells.Min(a => a.x) * floor.CellSize.x * floor.Scale.x) - margins.x);
        this.LimitRight = (int)(Math.Max(viewport.x, cells.Max(a => a.x + 1) * floor.CellSize.x * floor.Scale.x) + margins.x);
        this.LimitTop = (int)(Math.Min(0, cells.Min(a => a.y) * floor.CellSize.y * floor.Scale.x) - margins.y);
        this.LimitBottom = (int)(Math.Max(viewport.y, cells.Max(a => a.y + 1) * floor.CellSize.y * floor.Scale.x) + margins.y);
    }
}
