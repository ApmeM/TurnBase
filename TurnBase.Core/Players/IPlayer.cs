namespace TurnBase.Core;

public interface IPlayer
{
    Task<InitResponseModel> init(InitModel model);

    Task<MakeTurnResponseModel> makeTurn(MakeTurnModel model);
}
