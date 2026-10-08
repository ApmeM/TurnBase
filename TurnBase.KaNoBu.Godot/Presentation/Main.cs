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
        var field = mapUnit.GameToLaunch.Instance<LevelBase>();
        field.Initialize();
        field.Game.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel, Field2D>(new GDLogger()));
        this.AttachReplayStorageListener(field);
        var result = await this.StartGame(field);
        levelMap.LevelFinished(mapUnit, result);
    }

    private async void OnStartReplayGame()
    {
        if (this.lastReplay == null || this.lastReplay.Count == 0)
        {
            GD.PrintErr("No replay available to start.");
            return;
        }

        var field = this.Replay.Instance<GameField>();
        var replay = new ReplayGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel, Field2D>(new List<ICommunicationModel>(this.lastReplay))
        {
            playerTurnDelayAction = async () => await this.GetTree().CreateTimer(0.5f).ToMySignal(CommonSignals.Timeout)
        };

        field.Game = replay;
        field.Game.AddGameLogListener(field);
        field.Game.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel, Field2D>(new GDLogger()));

        await this.StartGame(field);
    }

    private async void OnStartCustomGame()
    {
        // server
        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            IsClient = false,
            GameId = "test",
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

        var game = this.GameField.Instance<GameField>();
        game.Game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            game,
            this.client,
            new GDLogger(),
            this.server,
            async (delay) => await this.GetTree().CreateTimer(delay / 1000f).ToMySignal(CommonSignals.Timeout));

        if (game.Game == null)
        {
            GD.PrintErr("TODO: Failed to create game popup.");
            return;
        }

        this.AttachReplayStorageListener(game);
        await this.StartGame(game);
    }

    private async void OnStartConnectGame()
    {
        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            IsClient = true,
            GameId = "test",
            ServerAddress = $"http://{this.serverIpInput.Text}:8080",
            PlayerTypes = new List<KaNoBuPlayerType> { (KaNoBuPlayerType)this.clientPlayer.GetSelectedId() }
        };

        var game = this.GameField.Instance<GameField>();
        game.Game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            game,
            this.client,
            new GDLogger(),
            this.server,
            async (delay) => await this.GetTree().CreateTimer(delay / 1000f).ToMySignal(CommonSignals.Timeout));

        this.AttachReplayStorageListener(game);
        await this.StartGame(game);
    }
    private CancellationTokenSource currentGameCancellationTokenSource;

    private async Task<bool> StartGame(GameField game)
    {
        if (game == null)
        {
            return false;
        }

        this.mainScreenButtonContainer.Visible = false;
        this.inGameButtonContainer.Visible = true;

        this.customPopup.Hide();

        this.AddChild(game);
        this.SetCameraLimits(game.Water);

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
            await game.Play(this.currentGameCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }

        return await this.EndGame(game);
    }

    private async void EndCurrentGame()
    {
        this.mainMenuPopup.Hide();
        this.currentGameCancellationTokenSource?.Cancel();
    }

    private async Task<bool> EndGame(GameField gameField)
    {
        var result = false;

        var fieldPlayerId = gameField.playerId;
        var winners = gameField.Winners?.ToHashSet();
        if (fieldPlayerId == -1)
        {
            this.winnerLabel.Text = $"The game is over.";
        }
        if (winners?.Contains(fieldPlayerId) ?? false)
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

        gameField.Game.Disconnect(gameField);
        gameField.QueueFree();

        // this.uI.Visible = true;
        this.staticCamera.Current = true;
        return result;
    }

    public void SetCameraLimits(TileMap field)
    {
        this.draggableCamera.SetCameraLimits(field, Vector2.Zero);
    }

    private void AttachReplayStorageListener(GameField field)
    {
        var replayMemoryStorageListener = new MemoryStorageEventListener<KaNoBuMoveNotificationModel, Field2D>();
        this.lastReplay = replayMemoryStorageListener.Events;
        field.Game.AddGameLogListener(replayMemoryStorageListener);
        this.replayButton.Visible = true;
    }
}
