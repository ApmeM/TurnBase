using Godot;

[Tool]
[SceneReference("WaveGenerator.tscn")]
public partial class WaveGenerator
{
    [Export]
    public Texture WaveItem;

    [Export]
    public float Speed = 10;

    [Export]
    public float Lifetime = 1;

    [Export]
    public float Interval = 0.2f;

    [Export]
    public Vector2 ScaleFrom = Vector2.One / 2;

    [Export]
    public Vector2 ScaleTo = Vector2.One;

    private float timeSinceLastWave = float.MinValue;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }
    
    public override async void _Process(float delta)
    {
        base._Process(delta);

        timeSinceLastWave += delta;
        if (timeSinceLastWave >= Interval)
        {
            timeSinceLastWave = 0f;

            var nodeLeft = new Sprite
            {
                Texture = WaveItem,
                GlobalPosition = this.GlobalPosition,
                GlobalRotation = this.GlobalRotation,
                ZIndex = this.ZIndex - 1,
                Scale = ScaleFrom
            };

            var nodeRight = new Sprite
            {
                Texture = WaveItem,
                GlobalPosition = this.GlobalPosition,
                GlobalRotation = this.GlobalRotation + Mathf.Pi,
                ZIndex = this.ZIndex - 1,
                Scale = ScaleFrom
            };

            this.AddChild(nodeLeft);
            this.AddChild(nodeRight);

            nodeLeft.SetAsToplevel(true);
            nodeRight.SetAsToplevel(true);

            var tween = new Tween();
            this.AddChild(tween);
            tween.InterpolateProperty(nodeLeft, "global_position", this.GlobalPosition, this.GlobalPosition + Vector2.Right.Rotated(this.GlobalRotation) * Speed * Lifetime, Lifetime);
            tween.InterpolateProperty(nodeRight, "global_position", this.GlobalPosition, this.GlobalPosition + Vector2.Right.Rotated(this.GlobalRotation + Mathf.Pi) * Speed * Lifetime, Lifetime);
            tween.InterpolateProperty(nodeLeft, "scale", ScaleFrom, ScaleTo, Lifetime);
            tween.InterpolateProperty(nodeRight, "scale", ScaleFrom, ScaleTo, Lifetime);
            tween.Start();
            await ToSignal(tween, "tween_all_completed");
            tween.QueueFree();

            nodeLeft.QueueFree();
            nodeRight.QueueFree();
        }
    }

    public void Start()
    {
        timeSinceLastWave = 0f;
    }

    public void Stop()
    {
        timeSinceLastWave = float.MinValue;
    }
}
