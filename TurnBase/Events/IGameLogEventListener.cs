namespace TurnBase
{
    public interface IGameLogEventListener<TInitResponseModel, TMoveResponseModel, TMoveNotificationModel> : IGameEventListener<TMoveNotificationModel>
    {
        void GameLogPlayerInit(int playerNumber, TInitResponseModel initResponseModel);
        void GameLogPlayerTurn(int playerNumber, TMoveResponseModel moveResponseModel, MoveValidationStatus status);
    }
}