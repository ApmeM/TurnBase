using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TurnBase.KaNoBu;

public enum UnitType
{
    Unknown,
    Stone,
    Scissor,
    Paper,
    Flag
}

public class Unit : Node2D
{
    public Queue<IUnitAction> PendingActions = new Queue<IUnitAction>();

    private KaNoBuFigure.FigureTypes unitType = KaNoBuFigure.FigureTypes.ShipPaper;
    private bool unitTypeRaw = true;
    private int playerNumber = 0;
    private bool playerNumberRaw = true;
    public Vector2? TargetPositionMap;

    private AtlasTexture shipTexture;
    private AtlasTexture shipTypeTexture;

    [Export]
    public Texture CannonBall;

    [Export]
    public float Speed = 100;

    [Export]
    public float Lifetime = 1;

    [Export]
    public KaNoBuFigure.FigureTypes UnitType
    {
        get => this.unitType;
        set { this.unitType = value; this.unitTypeRaw = true; }
    }

    [Export]
    public int PlayerNumber
    {
        get => this.playerNumber;
        set { this.playerNumber = value; this.playerNumberRaw = true; }
    }

    [Export]
    public bool IsSelected { get; set; }

    [Signal]
    public delegate void AllActionsDone();

    public bool IsDead { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        this.shipTexture = (AtlasTexture)this.GetNode<Sprite>("Ship").Texture;
        this.shipTypeTexture = (AtlasTexture)this.GetNode<Sprite>("ShipTypeFlag").Texture;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        this.GetNode<Sprite>("Flag").Visible = this.IsSelected;
        if (this.playerNumberRaw)
        {
            this.playerNumberRaw = false;
            this.shipTexture.Region = new Rect2(0, 120 * this.playerNumber, 66, 113);
        }

        if (this.unitTypeRaw)
        {
            this.unitTypeRaw = false;
            switch (this.unitType)
            {
                // case KaNoBuFigure.FigureTypes.Unknown:
                //     this.shipTypeTexture.Region = new Rect2(280, 170, 20, 20);
                //     break;
                case KaNoBuFigure.FigureTypes.ShipStone:
                    this.shipTypeTexture.Region = new Rect2(300, 170, 20, 20);
                    break;
                case KaNoBuFigure.FigureTypes.ShipScissors:
                    this.shipTypeTexture.Region = new Rect2(280, 190, 20, 20);
                    break;
                case KaNoBuFigure.FigureTypes.ShipPaper:
                    this.shipTypeTexture.Region = new Rect2(300, 190, 20, 20);
                    break;
                case KaNoBuFigure.FigureTypes.ShipFlag:
                    this.shipTypeTexture.Region = new Rect2(320, 170, 20, 20);
                    break;
            }
        }

        if (PendingActions.Count > 0)
        {
            var action = PendingActions.Peek();
            var isActionDone = action.Process(delta);
            if (isActionDone)
            {
                PendingActions.Dequeue();
                if (PendingActions.Count == 0)
                {
                    this.EmitSignal(nameof(AllActionsDone));
                }
            }
        }
    }

    public void UnitHit()
    {
        this.IsDead = true;
        this.PendingActions.Enqueue(new SinkUnitAction(this, this.shipTexture));
        this.TargetPositionMap = null;
    }

    public void Attack()
    {
        this.PendingActions.Enqueue(new ShootUnitAction(this));
    }

    public void MoveUnitTo(Vector2 newCell, Vector2 position)
    {
        this.TargetPositionMap = newCell;
        this.PendingActions.Enqueue(new MoveUnitAction(this, position));
    }

    public void RotateUnitTo(Vector2 lookAtPosition)
    {
        this.PendingActions.Enqueue(new RotateUnitAction(this, lookAtPosition));
    }

    public void CallbackForUnit(Action<Unit> callback)
    {
        this.PendingActions.Enqueue(new CallbackUnitAction(this, callback));
    }

    public void CancelActions()
    {
        this.PendingActions.Clear();
    }

    public async Task Shoot()
    {
        if (CannonBall == null)
        {
            return;
        }

        var cannonBall = new Sprite
        {
            Texture = CannonBall,
            Scale = Vector2.One * 2,
            ZIndex = 1
        };

        var tween = new Tween
        {
        };

        this.AddChild(tween);
        this.AddChild(cannonBall);
        tween.InterpolateProperty(cannonBall, "position", Vector2.Zero, Vector2.Right * this.Speed * this.Lifetime, this.Lifetime);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
        tween.QueueFree();
        cannonBall.QueueFree();
    }
}
