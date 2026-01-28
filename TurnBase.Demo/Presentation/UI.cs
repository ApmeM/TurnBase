using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBase;
using TurnBase.KaNoBu;


[SceneReference("UI.tscn")]
public partial class UI
{
    public override void _Ready()
    {
        this.FillMembers();

        this.gameType.Connect("item_selected", this, nameof(GameTypeChanged));
        this.startButton.Connect("pressed", this, nameof(StartButonClicked));

        this.serverIpInfo.Text = string.Join("\n", IP.GetLocalAddresses().Cast<string>().Where(a => !a.Contains(":")));
    }

    private void GameTypeChanged(int selectedId)
    {
        this.serverGameType.Visible = false;
        this.clientGameType.Visible = false;
        this.replayGameType.Visible = false;
        switch (this.gameType.GetSelectedId())
        {
            case 0:
                // server
                this.serverGameType.Visible = true;
                break;
            case 1:
                // client
                this.clientGameType.Visible = true;
                break;
            case 2:
                // Replay
                this.replayGameType.Visible = true;
                break;
            default:
                throw new Exception("Unknown game type");
        }
    }

    [Signal]
    public delegate void StartGameEventhandler();

    private void StartButonClicked()
    {
        this.EmitSignal(nameof(StartGameEventhandler));
    }

    private List<ICommunicationModel> lastReplay;

    public IGame BuildGame(GameField field)
    {
        var humanFound = false;
        IGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> kanobu;
        switch (this.gameType.GetSelectedId())
        {
            case 0:
                {
                    // server
                    var rules = new KaNoBuRules(8);
                    kanobu = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules, "test");

                    var playerTypes = new[]{
                    this.serverPlayer1,
                    this.serverPlayer2,
                    this.serverPlayer3,
                    this.serverPlayer4,
                };

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

                        kanobu.AddPlayer(BuildPlayer(playertype.GetSelectedId(), field, kanobu.GameId));
                    }

                    var memoryReplay = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
                    this.lastReplay = memoryReplay.Events;
                    kanobu.AddGameLogListener(memoryReplay);
                    this.gameType.SetItemDisabled(2, false);
                    break;
                }
            case 1:
                {
                    // Client
                    kanobu = new RemoteGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(this.client, $"http://{this.serverIpInput.Text}:8080", "test");
                    kanobu.AddPlayer(BuildPlayer(this.clientPlayer.GetSelectedId(), field, kanobu.GameId));

                    if (clientPlayer.GetSelectedId() == 1)
                    {
                        humanFound = true;
                    }

                    var memoryReplay = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
                    this.lastReplay = memoryReplay.Events;
                    kanobu.AddGameLogListener(memoryReplay);
                    this.gameType.SetItemDisabled(2, false);
                    break;
                }
            case 2:
                {
                    // Replay
                    if (lastReplay != null)
                    {
                        kanobu = new ReplayGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(lastReplay);
                    }
                    else
                    {
                        throw new InvalidOperationException("No replay found!");
                    }
                    break;
                }
            default:
                throw new InvalidOperationException("Unknown game type");
        }
        
        kanobu.AddGameLogListener(new ReadableLogger<KaNoBuMoveNotificationModel>(new GDLogger()));

        if(!humanFound)
        {
            kanobu.AddGameLogListener(field);
        }

        return kanobu;
    }

    public IPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel> BuildPlayer(int playerType, GameField field, string gameId)
    {
        switch (playerType)
        {
            case 0:
                // None
                return new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>();
            case 1:
                // Human
                return field;
            case 2:
                // Computer Easy
                var playerEasy = new KaNoBuPlayerEasy();
                return new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(playerEasy, 1, 300, this);
            case 3:
                // Remote
                this.server.StartServer();
                return new ServerPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(server, gameId);
            case 4:
                // Computer Medium
                var playerMedium = new KaNoBuPlayerMedium();
                return new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(playerMedium, 1, 300, this);
            default:
                throw new InvalidOperationException("Unknown Player Type");
        }
    }
}
