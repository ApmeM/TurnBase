using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;

[SceneReference("UI.tscn")]
public partial class UI
{
    [Export]
    public List<PackedScene> Levels;

    [Export]
    public PackedScene GameField;

    [Export]
    public PackedScene Replay;

    public override void _Ready()
    {
        this.FillMembers();

        this.startServerButton.Connect("pressed", this, nameof(StartButtonClicked));
        this.startClientButton.Connect("pressed", this, nameof(StartButtonClicked));
        this.startReplayButton.Connect("pressed", this, nameof(StartButtonClicked));
        this.startLevelsButton.Connect("pressed", this, nameof(StartButtonClicked));

        this.serverMyIpInfo.Text = "Your IP address: " + string.Join(", ", IP.GetLocalAddresses().Cast<string>().Where(a => !a.Contains(":")));
        this.clientMyIpInfo.Text = "Your IP address: " + string.Join(", ", IP.GetLocalAddresses().Cast<string>().Where(a => !a.Contains(":")));

        if (Levels != null)
        {
            for (int i = 0; i < Levels.Count; i++)
            {
                this.levelType.AddItem($"{i}");
            }
        }
    }

    [Signal]
    public delegate void StartGameEventhandler();

    private void StartButtonClicked()
    {
        this.EmitSignal(nameof(StartGameEventhandler));
    }

    private List<ICommunicationModel> lastReplay;

    public IGame BuildGame()
    {
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> kanobu;
        switch (this.gameType.CurrentTab)
        {
            case 0:
                {
                    // server
                    var field = this.GameField.Instance<GameField>();
                    this.GetParent().AddChild(field);

                    var rules = new KaNoBuRules((int)this.mapSizeSelector.Value);
                    rules.AllFiguresVisible = this.allShipsVisibleSelector.Pressed;
                    kanobu = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules, "test");

                    var playerTypes = new[]{
                        this.serverPlayer1,
                        this.serverPlayer2,
                        this.serverPlayer3,
                        this.serverPlayer4,
                    };

                    var humanFound = false;
                    foreach (var playertype in playerTypes)
                    {
                        if (playertype.GetSelectedId() == 1)
                        {
                            if (humanFound)
                            {
                                GD.Print("Only one human player is allowed.");
                                kanobu.AddPlayer(new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>());
                                continue;
                            }

                            humanFound = true;
                        }

                        kanobu.AddPlayer(BuildPlayer(playertype.GetSelectedId(), kanobu.GameId));
                    }

                    if (!humanFound)
                    {
                        kanobu.AddGameLogListener(field);
                    }

                    var memoryReplay = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
                    this.lastReplay = memoryReplay.Events;
                    kanobu.AddGameLogListener(memoryReplay);
                    this.startReplayButton.Disabled = false;
                    break;
                }
            case 1:
                {
                    // Client
                    var field = this.GameField.Instance<GameField>();
                    this.GetParent().AddChild(field);

                    kanobu = new RemoteGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(this.client, $"http://{this.serverIpInput.Text}:8080", "test");
                    kanobu.AddPlayer(BuildPlayer(this.clientPlayer.GetSelectedId(), kanobu.GameId));

                    if (clientPlayer.GetSelectedId() != 1)
                    {
                        kanobu.AddGameLogListener(field);
                    }

                    var memoryReplay = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
                    this.lastReplay = memoryReplay.Events;
                    kanobu.AddGameLogListener(memoryReplay);
                    this.startReplayButton.Disabled = false;
                    break;
                }
            case 2:
                {
                    // Replay
                    var field = this.Replay.Instance<GameField>();
                    this.GetParent().AddChild(field);

                    if (lastReplay == null)
                    {
                        throw new InvalidOperationException("No replay found!");
                    }
                    kanobu = new ReplayGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(lastReplay);
                    kanobu.AddGameLogListener(field);
                    break;
                }
            case 3:
                {
                    // Levels
                    var levelName = this.levelType.GetItemText(this.levelType.GetSelectedId());
                    var field = this.Levels[int.Parse(levelName)].Instance<LevelBase>();
                    this.GetParent().AddChild(field);
                    kanobu = field.Start();
                    break;
                }
            default:
                throw new InvalidOperationException("Unknown game type");
        }

        kanobu.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel>(new GDLogger()));

        return kanobu;
    }

    public IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> BuildPlayer(int playerType, string gameId)
    {
        switch (playerType)
        {
            case 0:
                // None
                return new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>();
            case 1:
                // Human
                var field = this.GetParent().GetNode<GameField>("GameField");
                return new TimeoutPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(
                    field,
                    async (delay) => await this.ToSignal(this.GetTree().CreateTimer(delay / 1000f), "timeout"),
                    1000,
                    60000);
            case 2:
                // Computer Easy
                var playerEasy = new KaNoBuPlayerEasy();
                return new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(
                    playerEasy,
                    async (delay) => await this.ToSignal(this.GetTree().CreateTimer(delay / 1000f), "timeout"),
                    1,
                    300);
            case 3:
                // Remote
                this.server.StartServer();
                var player = new ServerPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(server, gameId);
                return new TimeoutPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(
                    player,
                    async (delay) => await this.ToSignal(this.GetTree().CreateTimer(delay / 1000f), "timeout"),
                    600000,
                    60000);
            case 4:
                // Computer Medium
                var playerMedium = new KaNoBuPlayerMedium();
                return new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(
                    playerMedium,
                    async (delay) => await this.ToSignal(this.GetTree().CreateTimer(delay / 1000f), "timeout"),
                    1,
                    300);
            default:
                throw new InvalidOperationException("Unknown Player Type");
        }
    }
}
