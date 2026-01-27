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
        switch (this.gameType.GetSelectedId())
        {
            case 0:
                {
                    // server
                    var rules = new KaNoBuRules(8);
                    var kanobu = new Game<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(rules, "test");

                    var playerTypes = new[]{
                    this.serverPlayer1,
                    this.serverPlayer2,
                    this.serverPlayer3,
                    this.serverPlayer4,
                };

                    var humanFound = false;
                    foreach (var playertype in playerTypes)
                    {
                        switch (playertype.GetSelectedId())
                        {
                            case 0:
                                // None
                                kanobu.AddPlayer(new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>());
                                continue;
                            case 1:
                                // Human
                                if (humanFound)
                                {
                                    GD.Print("2 Human players are not implemented yet.");
                                    return null;
                                }
                                humanFound = true;
                                kanobu.AddPlayer(field);
                                continue;
                            case 2:
                                // Computer Easy
                                var playerEasy = new KaNoBuPlayerEasy();
                                kanobu.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(playerEasy, 1, 300, this));
                                continue;
                            case 3:
                                // Remote
                                this.server.StartServer();
                                var player = new ServerPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(server, kanobu.GameId);
                                kanobu.AddPlayer(player);
                                continue;
                            case 4:
                                // Computer Medium
                                var playerMedium = new KaNoBuPlayerMedium();
                                kanobu.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(playerMedium, 1, 300, this));
                                continue;
                            default:
                                throw new InvalidOperationException("Unknown Player Type");
                        }
                    }

                    if (!humanFound)
                    {
                        kanobu.AddGameLogListener(field);
                    }
                    var memoryReplay = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
                    this.lastReplay = memoryReplay.Events;
                    kanobu.AddGameLogListener(memoryReplay);
                    this.gameType.SetItemDisabled(2, false);
                    return kanobu;
                }
            case 1:
                {
                    // Client
                    var kanobu = new RemoteGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(this.client, $"http://{this.serverIpInput.Text}:8080", "test");
                    kanobu.AddPlayer(field);
                    var memoryReplay = new MemoryStorageEventListener<KaNoBuMoveNotificationModel>();
                    this.lastReplay = memoryReplay.Events;
                    kanobu.AddGameLogListener(memoryReplay);
                    this.gameType.SetItemDisabled(2, false);
                    return kanobu;
                }
            case 2:
                {
                    // Replay
                    if (lastReplay != null)
                    {
                        var kanobu = new ReplayGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(lastReplay);
                        kanobu.AddGameLogListener(field);
                        return kanobu;
                    }
                    else
                    {
                        throw new InvalidOperationException("No replay found!");
                    }
                }
            default:
                throw new InvalidOperationException("Unknown game type");
        }
    }
}
