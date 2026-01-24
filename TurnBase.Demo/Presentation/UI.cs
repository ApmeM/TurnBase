using System;
using System.CodeDom;
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
        switch (this.gameType.GetSelectedId())
        {
            case 0:
                // server
                this.serverGameType.Visible = true;
                this.clientGameType.Visible = false;
                break;
            case 1:
                // client
                this.serverGameType.Visible = false;
                this.clientGameType.Visible = true;
                break;
            default:
                throw new Exception("Unknown game type");
        }
    }

    [Signal]
    public delegate void StartGameEventhandler();

    private async void StartButonClicked()
    {
        this.EmitSignal(nameof(StartGameEventhandler));
    }
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
                                kanobu.AddPlayer(new PlayerLoose<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>());
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
                                kanobu.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(playerEasy, 1, 300, this));
                                continue;
                            case 3:
                                // Remote
                                this.server.StartServer();
                                var player = new ServerPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(server, kanobu.GameId);
                                kanobu.AddPlayer(player);
                                kanobu.AddGameListener(player);
                                continue;
                            case 4:
                                // Computer Medium
                                var playerMedium = new KaNoBuPlayerMedium();
                                kanobu.AddPlayer(new DelayedPlayer<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel>(playerMedium, 1, 300, this));
                                kanobu.AddGameListener(playerMedium);
                                continue;
                            default:
                                throw new InvalidOperationException("Unknown Player Type");
                        }
                    }

                    kanobu.AddGameListener(field);
                    if (!humanFound)
                    {
                        kanobu.AddGameLogListener(field);
                    }

                    return kanobu;
                }
            case 1:
                {
                    var kanobu = new RemoteGame<KaNoBuInitModel, KaNoBuInitResponseModel, KaNoBuMoveModel, KaNoBuMoveResponseModel, KaNoBuMoveNotificationModel>(this.client, $"http://{this.serverIpInput.Text}:8080", "test");
                    kanobu.SetPlayer(field);
                    return kanobu;
                }
            default:
                throw new InvalidOperationException("Unknown game type");
        }
    }
}
