using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TurnBase;

public class ServerPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> :
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>,
        IGameEventListener<TMoveNotificationModel>
{
    private readonly Server server;

    public string PlayerId { get; private set; }

    public ServerPlayer(Server server, string gameId)
    {
        this.server = server;
        this.PlayerId = Guid.NewGuid().ToString();
        this.server.RegisterPlayer(this.PlayerId, gameId);
    }

    public async Task<InitResponseModel<TInitResponseModel>> Init(
        InitModel<TInitModel> model)
    {
        this.server.Actions.PushModel(PlayerId, model);
        return await this.server.Actions.WaitResponse<InitResponseModel<TInitResponseModel>>(PlayerId);
    }

    public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(
        MakeTurnModel<TMoveModel> model)
    {
        this.server.Actions.PushModel(PlayerId, model);
        return await this.server.Actions.WaitResponse<MakeTurnResponseModel<TMoveResponseModel>>(PlayerId);
    }

    public void GameStarted()
    {
        this.server.Actions.PushModel(PlayerId, new GameStartedCommunicationModel());
    }

    public void GamePlayerInit(int playerNumber, string playerName)
    {
        this.server.Actions.PushModel(PlayerId, new GamePlayerInitCommunicationModel
        {
            playerNumber = playerNumber,
            playerName = playerName
        });
    }

    public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
    {
        this.server.Actions.PushModel(PlayerId, new GamePlayerTurnCommunicationModel
        {
            playerNumber = playerNumber,
            notification = notification
        });
    }

    public void GameTurnFinished()
    {
        this.server.Actions.PushModel(PlayerId, new GameTurnFinishedCommunicationModel());
    }

    public void GamePlayerDisconnected(int playerNumber)
    {
        this.server.Actions.PushModel(PlayerId, new GamePlayerDisconnectedCommunicationModel
        {
            playerNumber = playerNumber,
        });
    }

    public void GameFinished(List<int> winners)
    {
        this.server.Actions.PushModel(PlayerId, new GameFinishedCommunicationModel
        {
            winners = winners
        });
    }
}
