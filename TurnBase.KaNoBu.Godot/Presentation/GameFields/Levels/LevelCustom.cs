using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("LevelCustom.tscn")]
public partial class LevelCustom
{
    private TaskCompletionSource<IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>> gameCompletion;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.AddToGroup(Groups.Level);

        this.startCustomButton.Connect(CommonSignals.Pressed, this, nameof(OnStartCustomGame));
        this.startConnectButton.Connect(CommonSignals.Pressed, this, nameof(OnStartConnectGame));

        this.customPopup.Connect(nameof(CustomPopup.PopupClosed), this, nameof(OnSettingsClosed));

        this.serverMyIpInfo.Text = "Your IP address: " + string.Join(", ", IP.GetLocalAddresses().Cast<string>().Where(address => !address.Contains(":")));
        this.clientMyIpInfo.Text = "Your IP address: " + string.Join(", ", IP.GetLocalAddresses().Cast<string>().Where(a => !a.Contains(":")));
    }

    public override async Task<IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>> CreateGame()
    {
        if (this.gameCompletion != null)
        {
            throw new InvalidOperationException("Custom game configuration is already running.");
        }

        this.gameCompletion = new TaskCompletionSource<IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>>();
        this.customPopup.Show();
        var game = await this.gameCompletion.Task;
        this.customPopup.Hide();
        this.gameCompletion = null;

        return game;
    }

    protected override void InitializeGameField(Field2D mainField)
    {
        base.InitializeGameField(mainField);

        var center = new Vector2((mainField.Width - 1) / 2f, (mainField.Height - 1) / 2f);
        foreach (var unit in this.field.GetChildren().OfType<Unit>())
        {
            if (!unit.TargetPositionMap.HasValue)
            {
                continue;
            }

            var targetMap = unit.TargetPositionMap.Value;
            var direction = targetMap - center;
            var spawnMap = targetMap;

            if (Math.Abs(direction.x) >= Math.Abs(direction.y) && direction.x != 0)
            {
                spawnMap.x += Math.Sign(direction.x);
            }
            else if (direction.y != 0)
            {
                spawnMap.y += Math.Sign(direction.y);
            }
            else
            {
                // A ship exactly at the center has no outward direction; use the top edge.
                spawnMap.y -= 1;
            }

            var targetPosition = this.field.MapToWorld(targetMap) + this.field.CellSize / 2;
            unit.Position = this.field.MapToWorld(spawnMap) + this.field.CellSize / 2;
            _ = unit.RotateUnitToAnimation(targetPosition);
            _ = unit.MoveUnitToAnimation(targetPosition);
        }
    }

    private void OnStartCustomGame()
    {
        if (this.gameCompletion == null)
        {
            return;
        }

        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            IsClient = false,
            GameId = "local" + Guid.NewGuid().ToString(),
            ServerAddress = null,
            FieldSize = (int)this.mapSizeSelector.Value,
            WithDocks = this.withDocksSelector.Pressed,
            EnemyVisible = this.allShipsVisibleSelector.Pressed,
            MaxMovesPerTurn = (int)this.maxMovesPerTurnSelector.Value,
            PlayerTypes = new[]{
                this.serverPlayer1,
                this.serverPlayer2,
                this.serverPlayer3,
                this.serverPlayer4,
            }.Select(a => (KaNoBuPlayerType)a.GetSelectedId()).ToList()
        };

        var game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            this,
            this.client,
            new GDLogger(),
            this.server,
            async delay => await this.GetTree().CreateTimer(delay / 1000f).ToMySignal(CommonSignals.Timeout));

        this.gameCompletion.TrySetResult(game);
    }

    private async void OnStartConnectGame()
    {
        if (this.gameCompletion == null)
        {
            return;
        }

        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            IsClient = true,
            GameId = null,
            ServerAddress = $"http://{this.serverIpInput.Text}:8080",
            PlayerTypes = new List<KaNoBuPlayerType> { (KaNoBuPlayerType)this.clientPlayer.GetSelectedId() }
        };

        var game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            this,
            this.client,
            new GDLogger(),
            this.server,
            async (delay) => await this.GetTree().CreateTimer(delay / 1000f).ToMySignal(CommonSignals.Timeout));

        this.gameCompletion.TrySetResult(game);
    }

    private void OnSettingsClosed()
    {
        this.gameCompletion?.TrySetResult(null);
    }
}
