namespace TurnBase.Core;

public interface IPlayer
{
    Task<InitResponseModel> Init(int playerNumber, InitModel model);

    Task<MakeTurnResponseModel> MakeTurn(MakeTurnModel model);
}
