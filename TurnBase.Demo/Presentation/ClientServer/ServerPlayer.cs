using System;
using System.Threading.Tasks;
using Godot;
using TurnBase;

public class ServerPlayer<
    TInitModel,
    TInitResponseModel,
    TMoveModel,
    TMoveResponseModel>
    : IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>
{
    private readonly Server server;

    public string PlayerId { get; private set;}

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
}
