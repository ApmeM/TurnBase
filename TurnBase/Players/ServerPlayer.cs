using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TurnBase;

public interface IServer
{
    void RegisterPlayer(string playerId, string gameId);
    void SendRequest(string playerId, ICommunicationModel model);
    Task<T> SendRequest<T>(string playerId, ICommunicationModel model);
}

public class ServerPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel> :
        IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel, TMoveNotificationModel>
{
    private readonly IServer server;

    public string PlayerId { get; private set; }

    public ServerPlayer(IServer server, string gameId)
    {
        this.server = server;
        this.PlayerId = Guid.NewGuid().ToString();
        this.server.RegisterPlayer(this.PlayerId, gameId);
    }

    public async Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model)
    {
        return await this.server.SendRequest<InitResponseModel<TInitResponseModel>>(PlayerId, model);
    }

    public async Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model)
    {
        return await this.server.SendRequest<MakeTurnResponseModel<TMoveResponseModel>>(PlayerId, model);
    }

    public void GameStarted()
    {
        this.server.SendRequest(PlayerId, new GameStartedCommunicationModel());
    }

    public void GamePlayerInit(int playerNumber, string playerName)
    {
        this.server.SendRequest(PlayerId, new GamePlayerInitCommunicationModel
        {
            playerNumber = playerNumber,
            playerName = playerName
        });
    }

    public void PlayersInitialized()
    {
        this.server.SendRequest(PlayerId, new GamePlayersInitializedCommunicationModel());
    }

    public void GameLogCurrentField(IField field)
    {
        this.server.SendRequest(PlayerId, new GameLogCurrentFieldCommunicationModel
        {
            field = field
        });
    }

    public void GamePlayerTurn(int playerNumber, TMoveNotificationModel notification)
    {
        this.server.SendRequest(PlayerId, new GamePlayerTurnCommunicationModel<TMoveNotificationModel>
        {
            playerNumber = playerNumber,
            notification = notification
        });
    }

    public void GameTurnFinished()
    {
        this.server.SendRequest(PlayerId, new GameTurnFinishedCommunicationModel());
    }

    public void GamePlayerDisconnected(int playerNumber)
    {
        this.server.SendRequest(PlayerId, new GamePlayerDisconnectedCommunicationModel
        {
            playerNumber = playerNumber,
        });
    }

    public void GameFinished(List<int> winners)
    {
        this.server.SendRequest(PlayerId, new GameFinishedCommunicationModel
        {
            winners = winners
        });
    }
}
