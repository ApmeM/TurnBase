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
