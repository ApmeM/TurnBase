namespace TurnBase.Core;

public interface IPlayer<TInitModel, TInitResponseModel, TMoveModel, TMoveResponseModel>
{
    Task<InitResponseModel<TInitResponseModel>> Init(InitModel<TInitModel> model);

    Task<MakeTurnResponseModel<TMoveResponseModel>> MakeTurn(MakeTurnModel<TMoveModel> model);
}
