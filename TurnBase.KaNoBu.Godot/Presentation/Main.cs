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

    private List<ICommunicationModel> lastReplay;

    public override void _Ready()
    {
        this.FillMembers();

        this.levelMap.Connect(nameof(LevelMap.LevelSelected), this, nameof(OnStartLevelGame));
        this.replayButton.Connect(CommonSignals.Pressed, this, nameof(OnStartReplayGame));

        this.exitGameButton.Connect(CommonSignals.Pressed, this, nameof(EndCurrentGame));
        this.mainMenuButton.Connect(CommonSignals.Pressed, this, nameof(ShowMainMenuPopup));

        PlayerFailProtection<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>.logger = new GDLogger();
    }

    private void ShowMainMenuPopup()
    {
        this.mainMenuPopup.Show();
    }

    private async void OnStartLevelGame(LevelMapUnit mapUnit)
    {
        var godotPlayer = mapUnit.GameToLaunch.Instance<GameField>();
        this.AddChild(godotPlayer);

        var game = await godotPlayer.CreateGame();
        if (game == null)
        {
            godotPlayer.QueueFree();
            return;
        }

        game.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel>(new GDLogger()));
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
        this.AddChild(godotPlayer);

        var game = new ReplayGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(new List<ICommunicationModel>(this.lastReplay))
        {
            playerTurnDelayAction = async () => await this.GetTree().CreateTimer(0.5f).ToMySignal(CommonSignals.Timeout)
        };

        game.AddGameLogListener(godotPlayer);
        game.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel>(new GDLogger()));

        await this.StartGame(godotPlayer, game);
    }

    private CancellationTokenSource currentGameCancellationTokenSource;

    private async Task<bool> StartGame(
        GameField godotPlayer,
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> game)
    {
        if (godotPlayer == null)
        {
            return false;
        }

        this.mainMenuPopup.Hide();

        this.levelMap.Hide();

        this.mainScreenButtonContainer.Visible = false;
        this.inGameButtonContainer.Visible = true;

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
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> game)
    {
        this.mainMenuPopup.Hide();

        var result = false;

        var fieldPlayerId = godotPlayer.playerId;
        var winners = godotPlayer.Winners.ToHashSet();

        if (winners.Count == 0)
        {
            this.winnerLabel.Text = "The game is over. No winners.";
        } else
        if (fieldPlayerId == -1)
        {
            this.winnerLabel.Text = $"The game is over.\nWinners: {string.Join(", ", winners.OrderBy(a => a).Select(a => $"Player {a}"))}.";
        }
        else if (winners.Contains(fieldPlayerId))
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
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> game)
    {
        var replayMemoryStorageListener = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
        this.lastReplay = replayMemoryStorageListener.Events;
        game.AddGameLogListener(replayMemoryStorageListener);
        this.replayButton.Visible = true;
    }
}
