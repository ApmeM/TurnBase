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

    public GameField BuildGame()
    {
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> kanobu;
        GameField field;
        switch (this.gameType.CurrentTab)
        {
            case 0:
                {
                    // server
                    field = this.GameField.Instance<GameField>();

                    var rules = new KaNoBuRules((int)this.mapSizeSelector.Value);
                    rules.AllFiguresVisible = this.allShipsVisibleSelector.Pressed;
                    field.Game = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules, "test");

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
                                field.Game.AddPlayer(new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>());
                                continue;
                            }

                            humanFound = true;
                        }

                        field.Game.AddPlayer(BuildPlayer(playertype.GetSelectedId(), field));
                    }

                    if (!humanFound)
                    {
                        field.Game.AddGameLogListener(field);
                    }

                    var memoryReplay = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
                    this.lastReplay = memoryReplay.Events;
                    field.Game.AddGameLogListener(memoryReplay);
                    this.startReplayButton.Disabled = false;
                    break;
                }
            case 1:
                {
                    // Client
                    field = this.GameField.Instance<GameField>();

                    field.Game = new RemoteGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(this.client, $"http://{this.serverIpInput.Text}:8080", "test");
                    field.Game.AddPlayer(BuildPlayer(this.clientPlayer.GetSelectedId(), field));

                    if (clientPlayer.GetSelectedId() != 1)
                    {
                        field.Game.AddGameLogListener(field);
                    }

                    var memoryReplay = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
                    this.lastReplay = memoryReplay.Events;
                    field.Game.AddGameLogListener(memoryReplay);
                    this.startReplayButton.Disabled = false;
                    break;
                }
            case 2:
                {
                    // Replay
                    field = this.Replay.Instance<GameField>();

                    if (lastReplay == null)
                    {
                        throw new InvalidOperationException("No replay found!");
                    }
                    field.Game = new ReplayGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(lastReplay);
                    field.Game.AddGameLogListener(field);
                    break;
                }
            case 3:
                {
                    // Levels
                    var levelName = this.levelType.GetItemText(this.levelType.GetSelectedId());
                    field = this.Levels[int.Parse(levelName)].Instance<LevelBase>();
                    ((LevelBase)field).Initialize();
                    break;
                }
            default:
                throw new InvalidOperationException("Unknown game type");
        }

        field.Game.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel>(new GDLogger()));

        return field;
    }

    public IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> BuildPlayer(int playerType, GameField field)
    {
        switch (playerType)
        {
            case 0:
                // None
                return new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>();
            case 1:
                // Human
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
                var player = new ServerPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(server, field.Game.GameId);
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
