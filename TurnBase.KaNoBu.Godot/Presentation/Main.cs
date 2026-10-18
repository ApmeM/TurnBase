using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("Main.tscn")]
public partial class Main
{
    [Export]
    public PackedScene Replay;

    [Export]
    public PackedScene GameField;

    private List<ICommunicationModel> lastReplay;

    public override void _Ready()
    {
        this.FillMembers();

        this.startCustomButton.Connect(CommonSignals.Pressed, this, nameof(OnStartCustomGame));
        this.startConnectButton.Connect(CommonSignals.Pressed, this, nameof(OnStartConnectGame));
        this.levelMap.Connect(nameof(LevelMap.LevelSelected), this, nameof(OnStartLevelGame));
        this.replayButton.Connect(CommonSignals.Pressed, this, nameof(OnStartReplayGame));

        this.exitGameButton.Connect(CommonSignals.Pressed, this, nameof(EndCurrentGame));
        this.mainMenuButton.Connect(CommonSignals.Pressed, this, nameof(ShowMainMenuPopup));
        this.customButton.Connect(CommonSignals.Pressed, this, nameof(ShowCustomPopup));
        this.connectButton.Connect(CommonSignals.Pressed, this, nameof(ShowConnectPopup));

        this.serverMyIpInfo.Text = "Your IP address: " + string.Join(", ", IP.GetLocalAddresses().Cast<string>().Where(a => !a.Contains(":")));
        this.clientMyIpInfo.Text = "Your IP address: " + string.Join(", ", IP.GetLocalAddresses().Cast<string>().Where(a => !a.Contains(":")));

        PlayerFailProtection<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>.logger = new GDLogger();
    }

    private void ShowMainMenuPopup()
    {
        this.mainMenuPopup.Show();
    }

    private void ShowCustomPopup()
    {
        this.customPopup.Show();
    }

    private void ShowConnectPopup()
    {
        this.connectPopup.Show();
    }

    private async void OnStartLevelGame(LevelMapUnit mapUnit)
    {
        var godotPlayer = mapUnit.GameToLaunch.Instance<LevelBase>();

        var game = godotPlayer.Initialize();
        game.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel, KaNoBuField>(new GDLogger()));
        this.AttachReplayStorageListener(game);
        var result = await this.StartGame(godotPlayer, game);
        levelMap.LevelFinished(mapUnit, result);
    }

    private async void OnStartReplayGame()
    {
        if (this.lastReplay == null || this.lastReplay.Count == 0)
        {
            GD.PrintErr("No replay available to start.");
            return;
        }

        var godotPlayer = this.Replay.Instance<GameField>();
        var game = new ReplayGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField>(new List<ICommunicationModel>(this.lastReplay))
        {
            playerTurnDelayAction = async () => await this.GetTree().CreateTimer(0.5f).ToMySignal(CommonSignals.Timeout)
        };

        game.AddGameLogListener(godotPlayer);
        game.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel, KaNoBuField>(new GDLogger()));

        await this.StartGame(godotPlayer, game);
    }

    private async void OnStartCustomGame()
    {
        // server
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

        var godotPlayer = this.GameField.Instance<GameField>();
        var game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            godotPlayer,
            this.client,
            new GDLogger(),
            this.server,
            async (delay) => await this.GetTree().CreateTimer(delay / 1000f).ToMySignal(CommonSignals.Timeout));

        if (game == null)
        {
            GD.PrintErr("TODO: Failed to create game popup.");
            return;
        }

        this.AttachReplayStorageListener(game);
        await this.StartGame(godotPlayer, game);
    }

    private async void OnStartConnectGame()
    {
        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            IsClient = true,
            GameId = null,
            ServerAddress = $"http://{this.serverIpInput.Text}:8080",
            PlayerTypes = new List<KaNoBuPlayerType> { (KaNoBuPlayerType)this.clientPlayer.GetSelectedId() }
        };

        var godotPlayer = this.GameField.Instance<GameField>();
        var game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            godotPlayer,
            this.client,
            new GDLogger(),
            this.server,
            async (delay) => await this.GetTree().CreateTimer(delay / 1000f).ToMySignal(CommonSignals.Timeout));

        if (game == null)
        {
            GD.PrintErr("TODO: Failed to create game popup.");
            return;
        }

        this.AttachReplayStorageListener(game);
        await this.StartGame(godotPlayer, game);
    }
    private CancellationTokenSource currentGameCancellationTokenSource;

    private async Task<bool> StartGame(
        GameField godotPlayer,
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> game)
    {
        if (godotPlayer == null)
        {
            return false;
        }

        this.customPopup.Hide();
        this.connectPopup.Hide();
        this.mainMenuPopup.Hide();

        this.levelMap.Hide();

        this.mainScreenButtonContainer.Visible = false;
        this.inGameButtonContainer.Visible = true;

        this.AddChild(godotPlayer);
        this.SetCameraLimits(godotPlayer.Water);

        this.draggableCamera.Current = true;
        this.draggableCamera.Position = this.staticCamera.Position;
        this.draggableCamera.Scale = this.staticCamera.Scale;
        this.draggableCamera.Zoom = this.staticCamera.Zoom;

        if (this.currentGameCancellationTokenSource != null)
        {
            this.currentGameCancellationTokenSource.Cancel();
            this.currentGameCancellationTokenSource = null;
        }

        this.currentGameCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await game.Play(this.currentGameCancellationTokenSource.Token).WrapCancellation(this.currentGameCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }

        return await this.EndGame(godotPlayer, game);
    }

    private async void EndCurrentGame()
    {
        this.currentGameCancellationTokenSource?.Cancel();
    }

    private async Task<bool> EndGame(
        GameField godotPlayer,
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> game)
    {
        this.mainMenuPopup.Hide();

        var result = false;

        var fieldPlayerId = godotPlayer.playerId;
        var winners = godotPlayer.Winners?.ToHashSet();
        if (fieldPlayerId == -1)
        {
            this.winnerLabel.Text = $"The game is over.";
        }
        else if (winners?.Contains(fieldPlayerId) ?? false)
        {
            this.winnerLabel.Text = "You win.";
            result = true;
        }
        else
        {
            this.winnerLabel.Text = "You loose.";
        }

        this.mainScreenButtonContainer.Visible = true;
        this.inGameButtonContainer.Visible = false;

        this.gameOverPopup.Show();
        await this.ToSignal(this.gameOverPopup, nameof(CustomPopup.PopupClosed));

        game.Disconnect(godotPlayer);
        godotPlayer.QueueFree();

        this.levelMap.Show();
        this.staticCamera.Current = true;
        return result;
    }

    public void SetCameraLimits(TileMap field)
    {
        this.draggableCamera.SetCameraLimits(field, Vector2.Zero);
    }

    private void AttachReplayStorageListener(
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, KaNoBuField> game)
    {
        var replayMemoryStorageListener = new MemoryStorageEventListener<KaNoBuMoveNotificationModel, KaNoBuField>();
        this.lastReplay = replayMemoryStorageListener.Events;
        game.AddGameLogListener(replayMemoryStorageListener);
        this.replayButton.Visible = true;
    }
}
